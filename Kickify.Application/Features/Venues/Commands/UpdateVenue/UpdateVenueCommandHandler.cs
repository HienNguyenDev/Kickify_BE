using AutoMapper;
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
        private readonly IMapper _mapper;

        public UpdateVenueCommandHandler(
            IVenueRepository venueRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _venueRepository = venueRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            // Map properties from command to entity
            // Rule: null = keep old value, non-null (including empty string) = update
            _mapper.Map(request, venue);

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
