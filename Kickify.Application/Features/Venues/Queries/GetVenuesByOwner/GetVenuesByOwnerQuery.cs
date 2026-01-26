using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public record GetVenuesByOwnerQuery(
        Guid OwnerId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetVenuesByOwnerResponse>;
}
