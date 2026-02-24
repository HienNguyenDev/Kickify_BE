namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public record GetFieldsByOwnerResponse(
        List<OwnerFieldItemDto> Fields,
        int Total,
        int Page,
        int PageSize
    );

    public record OwnerFieldItemDto(
        Guid FieldId,
        Guid VenueId,
        string VenueName,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        bool IsActive,
        DateTime CreatedAt
    );
}
