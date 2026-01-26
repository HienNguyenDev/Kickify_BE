using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto
{
    public record UpdateVenuePhotoCommand(
        Guid PhotoId,
        Guid UserId,
        int? DisplayOrder
    ) : ICommand<UpdateVenuePhotoResponse>;
}
