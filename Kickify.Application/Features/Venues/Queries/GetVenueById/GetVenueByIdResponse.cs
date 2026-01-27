namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public record GetVenueByIdResponse(
        Guid VenueId,
        string VenueName,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        List<VenueFieldDto> Fields,
        List<OperatingHoursDto> OperatingHours,
        List<VenuePhotoDto> Photos,
        decimal WalletBalance,
        DateTime CreatedAt
    );

    public record VenueFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge
    );

    public record OperatingHoursDto(
        DayOfWeek DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime
    );

    public record VenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        bool IsPrimary
    );
}
