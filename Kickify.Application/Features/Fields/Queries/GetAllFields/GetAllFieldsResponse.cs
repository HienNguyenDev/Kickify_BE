namespace Kickify.Application.Features.Fields.Queries.GetAllFields
{
    public record GetAllFieldsResponse(
        List<FieldItemDto> Fields,
        int Total,
        int Page,
        int PageSize
    );

    public record FieldItemDto(
        Guid FieldId,
        Guid VenueId,
        string VenueName,
        string VenueAddress,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        bool IsActive,
        DateTime CreatedAt
    );
}
