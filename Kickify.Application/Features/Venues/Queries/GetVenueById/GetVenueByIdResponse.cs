namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public record GetVenueByIdResponse(
        Guid VenueId,
        string FieldName,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? Description,
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
        int MaxPlayers,
        decimal PricePerHour,
        string? Description
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
