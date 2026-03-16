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
        string? AdminNotes,
        decimal AverageRating,
        int TotalReviews,
        int TotalBookings,
        List<OwnerVenueFieldDto> Fields,
        List<OwnerVenueOperatingHoursDto> OperatingHours,
        List<OwnerVenuePhotoDto> Photos,
        decimal WalletBalance,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record OwnerVenueFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record OwnerVenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder
    );

    public record OwnerVenueOperatingHoursDto(
        string DayOfWeek,
        TimeSpan? OpenTime,
        TimeSpan? CloseTime
    );
}
