namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public record GetFieldsByVenueResponse(
        Guid VenueId,
        string VenueName,
        List<VenueFieldItemDto> Fields
    );

    public record VenueFieldItemDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        bool IsActive,
        DateTime CreatedAt
    );
}
