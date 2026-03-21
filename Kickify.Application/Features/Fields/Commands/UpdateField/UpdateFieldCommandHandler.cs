using AutoMapper;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public class UpdateFieldCommandHandler : ICommandHandler<UpdateFieldCommand, UpdateFieldResponse>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;

        public UpdateFieldCommandHandler(
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserContext userContext)
        {
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<Result<UpdateFieldResponse>> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Get field with tracking for update
            var field = await _fieldRepository.GetFieldWithVenueForUpdateAsync(request.FieldId, cancellationToken);

            if (field == null)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Check if user is the owner of the venue
            if (field.Venue?.OwnerId != userId)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.Unauthorized);
            }

            if (request.PeakDaysOfWeek != null)
            {
                var venueOpenDays = field.Venue.VenueOperatingHours
                    .Where(h => !h.IsClosed)
                    .Select(h => h.DayOfWeek)
                    .Distinct()
                    .ToList();

                var hasInvalidPeakDay = request.PeakDaysOfWeek.Any(day => !venueOpenDays.Contains(day));
                if (hasInvalidPeakDay)
                {
                    return Result.Failure<UpdateFieldResponse>(FieldErrors.PeakHourOnClosedVenueDay);
                }

                field.PeakDaysOfWeek = request.PeakDaysOfWeek.Distinct().ToList();
            }

            // Map properties from command to entity
            // Rule: null = keep old value, non-null (including empty string) = update
            _mapper.Map(request, field);

            // Handle FieldType enum separately (since it's a string in command)
            if (request.FieldType != null)
            {
                if (Enum.TryParse<FieldType>(request.FieldType, true, out var fieldType))
                {
                    field.FieldType = fieldType;
                }
            }

            if (request.IsPeakHourSurchargePercentage.HasValue)
            {
                field.IsPeakHourSurchargePercentage = request.IsPeakHourSurchargePercentage.Value;
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

            _fieldRepository.Update(field);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateFieldResponse(
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
                field.IsActive,
                field.UpdatedAt
            ));
        }
    }
}
