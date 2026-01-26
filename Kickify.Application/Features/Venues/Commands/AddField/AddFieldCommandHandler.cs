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
            var venue = await _venueRepository.GetByIdAsync(request.VenueId);
            if (venue == null)
            {
                return Result.Failure<AddFieldResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Parse field type
            if (!Enum.TryParse<FieldType>(request.FieldType, true, out var fieldType))
            {
                return Result.Failure<AddFieldResponse>(VenueErrors.InvalidFieldType(request.FieldType));
            }

            // Create new field
            var field = new Field
            {
                FieldId = Guid.NewGuid(),
                VenueId = request.VenueId,
                FieldName = request.Name,
                FieldType = fieldType,
                HourlyRate = request.PricePerHour,
                CreatedAt = DateTime.UtcNow
            };

            await _fieldRepository.AddAsync(field);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new AddFieldResponse(
                field.FieldId,
                field.VenueId,
                field.FieldName,
                field.FieldType.ToString(),
                0, // MaxPlayers not in entity
                field.HourlyRate,
                null, // Description not in entity
                field.CreatedAt
            ));
        }
    }
}
