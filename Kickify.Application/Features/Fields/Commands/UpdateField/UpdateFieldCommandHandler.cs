using AutoMapper;
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

        public UpdateFieldCommandHandler(
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
