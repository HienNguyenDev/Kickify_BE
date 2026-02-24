using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public record GetVenuesByOwnerQuery(
        string? SearchName = null,
        string? Status = null,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetVenuesByOwnerResponse>;
}
