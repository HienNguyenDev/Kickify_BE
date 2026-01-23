using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public class UpdateFieldCommandHandler : IRequestHandler<UpdateFieldCommand, Result<UpdateFieldResponse>>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateFieldCommandHandler(
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork)
        {
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateFieldResponse>> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
            // Get field with tracking for update
            var field = await _fieldRepository.GetFieldWithVenueForUpdateAsync(request.FieldId, cancellationToken);

            if (field == null)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Check if user is the owner of the venue
            if (field.Venue?.OwnerId != request.UserId)
            {
                return Result.Failure<UpdateFieldResponse>(FieldErrors.Unauthorized);
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.FieldName))
            {
                field.FieldName = request.FieldName;
            }

            if (!string.IsNullOrEmpty(request.FieldType))
            {
                if (Enum.TryParse<FieldType>(request.FieldType, true, out var fieldType))
                {
                    field.FieldType = fieldType;
                }
            }

            if (request.SurfaceType != null)
            {
                field.SurfaceType = request.SurfaceType;
            }

            if (request.HourlyRate.HasValue)
            {
                field.HourlyRate = request.HourlyRate.Value;
            }

            if (request.PeakHourSurcharge.HasValue)
            {
                field.PeakHourSurcharge = request.PeakHourSurcharge.Value;
            }

            if (request.IsActive.HasValue)
            {
                field.IsActive = request.IsActive.Value;
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
                field.IsActive,
                field.UpdatedAt
            ));
        }
    }
}
