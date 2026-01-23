using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public record GetVenuesByOwnerQuery(
        Guid OwnerId,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetVenuesByOwnerResponse>>;
}
