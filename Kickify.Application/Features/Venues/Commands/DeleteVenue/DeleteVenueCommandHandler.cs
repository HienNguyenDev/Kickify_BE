using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public class DeleteVenueCommandHandler : IRequestHandler<DeleteVenueCommand, Result<DeleteVenueResponse>>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVenueCommandHandler(
            IVenueRepository venueRepository,
            IUnitOfWork unitOfWork)
        {
            _venueRepository = venueRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeleteVenueResponse>> Handle(DeleteVenueCommand request, CancellationToken cancellationToken)
        {
            // Get venue with tracking for delete
            var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<DeleteVenueResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Check if user is the owner
            if (venue.OwnerId != request.UserId)
            {
                return Result.Failure<DeleteVenueResponse>(VenueErrors.Unauthorized);
            }

            _venueRepository.Remove(venue);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteVenueResponse(request.VenueId, true));
        }
    }
}
