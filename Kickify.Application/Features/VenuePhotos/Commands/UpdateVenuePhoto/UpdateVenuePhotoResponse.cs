namespace Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto
{
    public record UpdateVenuePhotoResponse(
        Guid PhotoId,
        Guid VenueId,
        string PhotoUrl,
        int DisplayOrder
    );
}
