namespace Kickify.Application.Features.VenuePhotos.Commands.DeleteVenuePhoto
{
    public record DeleteVenuePhotoResponse(
        Guid PhotoId,
        bool Success,
        string Message
    );
}
