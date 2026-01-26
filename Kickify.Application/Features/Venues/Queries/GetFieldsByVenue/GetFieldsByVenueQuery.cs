using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public record GetFieldsByVenueQuery(Guid VenueId) : IQuery<GetFieldsByVenueResponse>;
}
