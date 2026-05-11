using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public class AddFieldCommandHandler : ICommandHandler<AddFieldCommand, AddFieldResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddFieldCommandHandler(
            IVenueRepository venueRepository,
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork)
        {
            _venueRepository = venueRepository;
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AddFieldResponse>> Handle(AddFieldCommand request, CancellationToken cancellationToken)
        {
            // Verify venue exists
            var venue = await _venueRepository.GetVenueWithDetailsAsync(request.VenueId, cancellationToken);
            if (venue == null)
            {
                return Result.Failure<AddFieldResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Parse field type
            if (!Enum.TryParse<FieldType>(request.FieldType, true, out var fieldType))
            {
                return Result.Failure<AddFieldResponse>(VenueErrors.InvalidFieldType(request.FieldType));
            }

            var venueOpenDays = venue.VenueOperatingHours
                .Where(h => !h.IsClosed)
                .Select(h => h.DayOfWeek)
                .Distinct()
                .ToList();

            var peakHours = new List<FieldPeakHour>();
            if (request.PeakHours is { Count: > 0 })
            {
                foreach (var peakHourDto in request.PeakHours)
                {
                    if (!TimeSpan.TryParse(peakHourDto.StartTime, out var startTime) ||
                        !TimeSpan.TryParse(peakHourDto.EndTime, out var endTime))
                    {
                        return Result.Failure<AddFieldResponse>(FieldErrors.InvalidPeakHourTimeRange);
                    }

                    var parsedDays = peakHourDto.ApplicableDays
                        .Select(day => Enum.TryParse<DayOfWeekEnum>(day, true, out var parsedDay)
                            ? (DayOfWeekEnum?)parsedDay
                            : null)
                        .Where(day => day.HasValue)
                        .Select(day => day!.Value)
                        .Distinct()
                        .ToList();

                    if (parsedDays.Count != peakHourDto.ApplicableDays.Count)
                    {
                        return Result.Failure<AddFieldResponse>(FieldErrors.PeakHourOnClosedVenueDay);
                    }

                    foreach (var parsedDay in parsedDays)
                    {
                        var operatingHour = venue.VenueOperatingHours.FirstOrDefault(h => h.DayOfWeek == parsedDay && !h.IsClosed);
                        if (operatingHour == null)
                        {
                            return Result.Failure<AddFieldResponse>(FieldErrors.PeakHourOnClosedVenueDay);
                        }

                        if (operatingHour.OpenTime.HasValue && operatingHour.CloseTime.HasValue)
                        {
                            if (startTime < operatingHour.OpenTime.Value || endTime > operatingHour.CloseTime.Value)
                            {
                                return Result.Failure<AddFieldResponse>(FieldErrors.PeakHourOutsideOperatingHours);
                            }
                        }
                        else
                        {
                            return Result.Failure<AddFieldResponse>(FieldErrors.PeakHourOutsideOperatingHours);
                        }
                    }

                    peakHours.Add(new FieldPeakHour
                    {
                        Id = Guid.NewGuid(),
                        StartTime = startTime,
                        EndTime = endTime,
                        SurchargeAmount = peakHourDto.SurchargeAmount,
                        IsPercentage = peakHourDto.IsPercentage,
                        ApplicableDays = parsedDays
                    });
                }
            }

            // Create new field
            var field = new Field
            {
                FieldId = Guid.NewGuid(),
                VenueId = request.VenueId,
                FieldName = request.Name,
                FieldType = fieldType,
                SurfaceType = request.SurfaceType,
                HourlyRate = request.HourlyRate,
                WeekendSurcharge = request.WeekendSurcharge,
                HolidaySurcharge = request.HolidaySurcharge,
                CreatedAt = DateTime.UtcNow,
                IsWeekendSurchargePercentage = request.IsWeekendSurchargePercentage ?? false,
                IsHolidaySurchargePercentage = request.IsHolidaySurchargePercentage ?? false,
                PeakHours = peakHours
            };

            await _fieldRepository.AddAsync(field);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new AddFieldResponse(
                field.FieldId,
                field.VenueId,
                field.FieldName,
                field.FieldType.ToString(),
                field.SurfaceType,
                field.HourlyRate,
                field.WeekendSurcharge,
                field.HolidaySurcharge,
                field.PeakHours.Select(ph => new AddFieldPeakHourResponseDto(
                    ph.Id,
                    ph.StartTime,
                    ph.EndTime,
                    ph.SurchargeAmount,
                    ph.IsPercentage,
                    ph.ApplicableDays
                )).ToList(),
                field.IsWeekendSurchargePercentage,
                field.IsHolidaySurchargePercentage,
                field.CreatedAt
            ));
        }
    }
}
