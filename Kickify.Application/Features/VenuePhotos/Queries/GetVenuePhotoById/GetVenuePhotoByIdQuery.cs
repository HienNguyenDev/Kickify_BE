using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotoById
{
    public record GetVenuePhotoByIdQuery(
        Guid PhotoId
    ) : IQuery<GetVenuePhotoByIdResponse>;
}
