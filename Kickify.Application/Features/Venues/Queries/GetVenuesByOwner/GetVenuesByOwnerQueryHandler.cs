using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public class GetVenuesByOwnerQueryHandler : IQueryHandler<GetVenuesByOwnerQuery, GetVenuesByOwnerResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public GetVenuesByOwnerQueryHandler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<Result<GetVenuesByOwnerResponse>> Handle(GetVenuesByOwnerQuery request, CancellationToken cancellationToken)
        {
            var (venues, total) = await _venueRepository.GetVenuesByOwnerPagedAsync(
                request.OwnerId,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var venueItems = venues.Select(v => new OwnerVenueItemDto(
                v.VenueId,
                v.VenueName,
                v.Address,
                v.Latitude,
                v.Longitude,
                v.ContactPhone,
                v.ContactEmail,
                v.Description,
                v.Amenities,
                v.Status.ToString(),
                v.AverageRating,
                v.TotalReviews,
                v.Fields?.Count ?? 0,
                v.VenueWallet?.Balance ?? 0,
                v.CreatedAt
            )).ToList();

            var response = new GetVenuesByOwnerResponse(
                venueItems,
                total,
                request.Page,
                request.PageSize
            );

            return Result.Success(response);
        }
    }
}
