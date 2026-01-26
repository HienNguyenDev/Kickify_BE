namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotoById
{
    public record GetVenuePhotoByIdResponse(
        Guid PhotoId,
        Guid VenueId,
        string PhotoUrl,
        int DisplayOrder,
        DateTime CreatedAt
    );
}
