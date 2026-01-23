using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public record GetFieldsByVenueQuery(Guid VenueId) : IRequest<Result<GetFieldsByVenueResponse>>;
}
