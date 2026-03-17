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

            // Auto-populate PeakDaysOfWeek: lấy các ngày mở cửa từ VenueOperatingHours
            var peakDaysOfWeek = venue.VenueOperatingHours
                .Where(h => !h.IsClosed)
                .Select(h => h.DayOfWeek)
                .Distinct()
                .ToList();

            // Create new field
            var field = new Field
            {
                FieldId = Guid.NewGuid(),
                VenueId = request.VenueId,
                FieldName = request.Name,
                FieldType = fieldType,
                SurfaceType = request.SurfaceType,
                HourlyRate = request.HourlyRate,
                PeakHourSurcharge = request.PeakHourSurcharge,
                PeakStartTime = request.PeakStartTime,
                PeakEndTime = request.PeakEndTime,
                WeekendSurcharge = request.WeekendSurcharge,
                HolidaySurcharge = request.HolidaySurcharge,
                CreatedAt = DateTime.UtcNow,
                PeakDaysOfWeek = peakDaysOfWeek,
                IsPeakHourSurchargePercentage = false,
                IsWeekendSurchargePercentage = false,
                IsHolidaySurchargePercentage = false
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
                field.PeakHourSurcharge,
                field.PeakStartTime,
                field.PeakEndTime,
                field.WeekendSurcharge,
                field.HolidaySurcharge,
                field.PeakDaysOfWeek,
                field.IsPeakHourSurchargePercentage,
                field.IsWeekendSurchargePercentage,
                field.IsHolidaySurchargePercentage,
                field.CreatedAt
            ));
        }
    }
}
