using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public class UpdateVenueCommandHandler : IRequestHandler<UpdateVenueCommand, Result<UpdateVenueResponse>>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVenueCommandHandler(
            IVenueRepository venueRepository,
            IUnitOfWork unitOfWork)
        {
            _venueRepository = venueRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateVenueResponse>> Handle(UpdateVenueCommand request, CancellationToken cancellationToken)
        {
            // Get venue with tracking for update
            var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<UpdateVenueResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Check if user is the owner
            if (venue.OwnerId != request.UserId)
            {
                return Result.Failure<UpdateVenueResponse>(VenueErrors.Unauthorized);
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name))
            {
                venue.VenueName = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                venue.Address = request.Address;
            }

            if (request.Latitude.HasValue)
            {
                venue.Latitude = request.Latitude.Value;
            }

            if (request.Longitude.HasValue)
            {
                venue.Longitude = request.Longitude.Value;
            }

            if (request.ContactPhone != null)
            {
                venue.ContactPhone = request.ContactPhone;
            }

            if (request.ContactEmail != null)
            {
                venue.ContactEmail = request.ContactEmail;
            }

            if (request.Description != null)
            {
                venue.Description = request.Description;
            }

            if (request.Amenities != null)
            {
                venue.Amenities = request.Amenities;
            }

            venue.UpdatedAt = DateTime.UtcNow;

            _venueRepository.Update(venue);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateVenueResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude,
                venue.Longitude,
                venue.ContactPhone,
                venue.ContactEmail,
                venue.Description,
                venue.Amenities,
                venue.UpdatedAt
            ));
        }
    }
}
