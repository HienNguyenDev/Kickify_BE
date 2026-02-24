using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto
{
    public record UpdateVenuePhotoCommand(
        Guid PhotoId,
        int? DisplayOrder
    ) : ICommand<UpdateVenuePhotoResponse>;
}
