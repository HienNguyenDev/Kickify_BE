using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public record GetVenueByIdQuery(Guid VenueId) : IQuery<GetVenueByIdResponse>;
}
