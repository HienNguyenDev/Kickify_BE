namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public record GetVenuesByOwnerResponse(
        List<OwnerVenueItemDto> Venues,
        int Total,
        int Page,
        int PageSize
    );

    public record OwnerVenueItemDto(
        Guid VenueId,
        string Name,
        string Address,
        decimal? Latitude,
        decimal? Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        string Status,
        decimal AverageRating,
        int TotalReviews,
        int FieldCount,
        decimal WalletBalance,
        DateTime CreatedAt
    );
}
