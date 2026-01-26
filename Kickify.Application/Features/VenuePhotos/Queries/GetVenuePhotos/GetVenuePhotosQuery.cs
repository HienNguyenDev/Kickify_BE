using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotos
{
    public record GetVenuePhotosQuery(
        Guid VenueId
    ) : IQuery<GetVenuePhotosResponse>;
}
