namespace Kickify.Application.Features.VenuePhotos.Commands.UploadVenuePhotos
{
    public record UploadVenuePhotosResponse(
        Guid VenueId,
        List<VenuePhotoDto> Photos
    );

    public record VenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder
    );
}
