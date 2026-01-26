using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public class DeleteFieldCommandHandler : ICommandHandler<DeleteFieldCommand, DeleteFieldResponse>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteFieldCommandHandler(
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork)
        {
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeleteFieldResponse>> Handle(DeleteFieldCommand request, CancellationToken cancellationToken)
        {
            // Get field with tracking for delete
            var field = await _fieldRepository.GetFieldWithVenueForUpdateAsync(request.FieldId, cancellationToken);

            if (field == null)
            {
                return Result.Failure<DeleteFieldResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Check if user is the owner of the venue
            if (field.Venue?.OwnerId != request.UserId)
            {
                return Result.Failure<DeleteFieldResponse>(FieldErrors.Unauthorized);
            }

            _fieldRepository.Remove(field);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteFieldResponse(request.FieldId, true));
        }
    }
}
