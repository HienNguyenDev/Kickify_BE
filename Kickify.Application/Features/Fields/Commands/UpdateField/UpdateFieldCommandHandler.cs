using AutoMapper;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public class UpdateFieldCommandHandler : ICommandHandler<UpdateFieldCommand, UpdateFieldResponse>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ILogger<UpdateFieldCommandHandler> _logger;
         public UpdateFieldCommandHandler(
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserContext userContext,
            ILogger<UpdateFieldCommandHandler> logger)
        {
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<UpdateFieldResponse>> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
          try
          {
            _logger.LogWarning("=== UpdateField [1] START === FieldId: {FieldId}", request.FieldId);
            
            var userId = _userContext.UserId;
            _logger.LogWarning("=== UpdateField [2] UserId: {UserId}", userId);
            
            // Get field with tracking for update
            var field = await _fieldRepository.GetFieldWithVenueForUpdateAsync(request.FieldId, cancellationToken);
            _logger.LogWarning("=== UpdateField [3] Field loaded: {Found}, PeakHours count: {Count}", 
                field != null, field?.PeakHours?.Count ?? -1);

            if (field == null)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Check if user is the owner of the venue
            if (field.Venue?.OwnerId != userId)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.Unauthorized);
            }
            _logger.LogWarning("=== UpdateField [4] Auth passed ===");

            // Map properties from command to entity
            _mapper.Map(request, field);
            _logger.LogWarning("=== UpdateField [5] AutoMapper done ===");

            // Handle FieldType enum separately
            if (request.FieldType != null)
            {
                if (Enum.TryParse<FieldType>(request.FieldType, true, out var fieldType))
                {
                    field.FieldType = fieldType;
                }
            }
            _logger.LogWarning("=== UpdateField [6] FieldType done ===");

            if (request.PeakHours != null)
            {
                _logger.LogWarning("=== UpdateField [7] Processing PeakHours, VenueOperatingHours count: {Count}", 
                    field.Venue?.VenueOperatingHours?.Count ?? -1);
                    
                var venueOpenDays = field.Venue.VenueOperatingHours
                    .Where(h => !h.IsClosed)
                    .Select(h => h.DayOfWeek)
                    .Distinct()
                    .ToList();
                _logger.LogWarning("=== UpdateField [8] VenueOpenDays: {Days}", string.Join(",", venueOpenDays));

                var newPeakHours = new List<FieldPeakHour>();
                foreach (var peakHourDto in request.PeakHours)
                {
                    if (!TimeSpan.TryParse(peakHourDto.StartTime, out var startTime) ||
                        !TimeSpan.TryParse(peakHourDto.EndTime, out var endTime) ||
                        startTime >= endTime)
                    {
                        return Result.Failure<UpdateFieldResponse>(FieldErrors.InvalidPeakHourTimeRange);
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
                        _logger.LogWarning("=== UpdateField REJECTED: PeakHourOnClosedVenueDay. ParsedDays: {Parsed}, ApplicableDays: {Applicable}",
                            string.Join(",", parsedDays), string.Join(",", peakHourDto.ApplicableDays));
                        return Result.Failure<UpdateFieldResponse>(FieldErrors.PeakHourOnClosedVenueDay);
                    }

                    foreach (var parsedDay in parsedDays)
                    {
                        var operatingHour = field.Venue.VenueOperatingHours.FirstOrDefault(h => h.DayOfWeek == parsedDay && !h.IsClosed);
                        if (operatingHour == null)
                        {
                            return Result.Failure<UpdateFieldResponse>(FieldErrors.PeakHourOnClosedVenueDay);
                        }

                        if (operatingHour.OpenTime.HasValue && operatingHour.CloseTime.HasValue)
                        {
                            if (startTime < operatingHour.OpenTime.Value || endTime > operatingHour.CloseTime.Value)
                            {
                                return Result.Failure<UpdateFieldResponse>(FieldErrors.PeakHourOutsideOperatingHours);
                            }
                        }
                        else
                        {
                            return Result.Failure<UpdateFieldResponse>(FieldErrors.PeakHourOutsideOperatingHours);
                        }
                    }

                    newPeakHours.Add(new FieldPeakHour
                    {
                        FieldId = field.FieldId,
                        StartTime = startTime,
                        EndTime = endTime,
                        SurchargeAmount = peakHourDto.SurchargeAmount,
                        IsPercentage = peakHourDto.IsPercentage,
                        ApplicableDays = parsedDays
                    });
                }
                _logger.LogWarning("=== UpdateField [9] NewPeakHours built: {Count}", newPeakHours.Count);

                field.PeakHours.Clear();
                _logger.LogWarning("=== UpdateField [10] PeakHours cleared ===");
                foreach (var peakHour in newPeakHours)
                {
                    field.PeakHours.Add(peakHour);
                }
                _logger.LogWarning("=== UpdateField [11] PeakHours added ===");
            }

            if (request.IsWeekendSurchargePercentage.HasValue)
            {
                field.IsWeekendSurchargePercentage = request.IsWeekendSurchargePercentage.Value;
            }

            if (request.IsHolidaySurchargePercentage.HasValue)
            {
                field.IsHolidaySurchargePercentage = request.IsHolidaySurchargePercentage.Value;
            }

            field.UpdatedAt = DateTime.UtcNow;
            _logger.LogWarning("=== UpdateField [12] About to SaveChanges ===");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("=== UpdateField [13] SaveChanges DONE ===");

            var response = new UpdateFieldResponse(
                field.FieldId,
                field.VenueId,
                field.FieldName,
                field.FieldType.ToString(),
                field.SurfaceType,
                field.HourlyRate,
                field.WeekendSurcharge,
                field.HolidaySurcharge,
                field.PeakHours.Select(ph => new UpdateFieldPeakHourResponseDto(
                    ph.Id,
                    ph.StartTime,
                    ph.EndTime,
                    ph.SurchargeAmount,
                    ph.IsPercentage,
                    ph.ApplicableDays
                )).ToList(),
                field.IsWeekendSurchargePercentage,
                field.IsHolidaySurchargePercentage,
                field.IsActive,
                field.UpdatedAt
            );
            _logger.LogWarning("=== UpdateField [14] Response built, SUCCESS ===");
            return Result.Success(response);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "=== UpdateField FAILED === Message: {Message}, InnerException: {Inner}",
                ex.Message,
                ex.InnerException?.Message ?? "none");
            throw;
          }
        }
    }
}
