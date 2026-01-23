namespace Kickify.Application.Features.Fields.Queries.GetAllFields
{
    public record GetAllFieldsResponse(
        List<FieldItemDto> Fields,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
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
