using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenuePhotos.Commands.DeleteVenuePhoto
{
    public record DeleteVenuePhotoCommand(
        Guid PhotoId
    ) : ICommand<DeleteVenuePhotoResponse>;
}
