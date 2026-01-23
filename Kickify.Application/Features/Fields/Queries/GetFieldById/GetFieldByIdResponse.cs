namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public record GetFieldByIdResponse(
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
        List<OperatingHourDto> OperatingHours,
        DateTime CreatedAt
    );

    public record OperatingHourDto(
        DayOfWeek DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime,
        bool IsClosed
    );
}
