namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public record GetAllVenuesResponse(
        List<VenueItemDto> Venues,
        int Total,
        int Page,
        int PageSize
    );

    public record VenueItemDto(
        Guid VenueId,
        string Name,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? Description,
        List<FieldSummaryDto> Fields,
        string? PrimaryPhotoUrl,
        DateTime CreatedAt
    );

    public record FieldSummaryDto(
        Guid FieldId,
        string Name,
        string FieldType,
        decimal PricePerHour
    );
}
