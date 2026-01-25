namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotos
{
    public record GetVenuePhotosResponse(
        Guid VenueId,
        List<VenuePhotoItemDto> Photos
    );

    public record VenuePhotoItemDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder,
        DateTime CreatedAt
    );
}
