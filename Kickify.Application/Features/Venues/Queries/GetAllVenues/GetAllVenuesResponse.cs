namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public record GetAllVenuesResponse(
        List<VenueItemDto> Venues,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
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
        string FieldName,
        string FieldType,
        decimal PricePerHour
    );
}
