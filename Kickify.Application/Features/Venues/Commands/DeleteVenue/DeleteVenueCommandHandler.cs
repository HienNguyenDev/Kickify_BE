using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public class DeleteVenueCommandHandler : ICommandHandler<DeleteVenueCommand, DeleteVenueResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public DeleteVenueCommandHandler(
            IVenueRepository venueRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _venueRepository = venueRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<DeleteVenueResponse>> Handle(DeleteVenueCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Get venue with tracking for delete
            var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<DeleteVenueResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Check if user is the owner
            if (venue.OwnerId != userId)
            {
                return Result.Failure<DeleteVenueResponse>(VenueErrors.Unauthorized);
            }

            _venueRepository.Remove(venue);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteVenueResponse(request.VenueId, true));
        }
    }
}
