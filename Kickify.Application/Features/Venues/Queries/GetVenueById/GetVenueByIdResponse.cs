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
        string Status,
        string? AdminNotes,
        decimal AverageRating,
        int TotalReviews,
        int TotalBookings,
        VenueOwnerDto Owner,
        List<IgnoredHolidayDto> IgnoredHolidays,
        List<VenueFieldDto> Fields,
        List<OperatingHoursDto> OperatingHours,
        List<VenuePhotoDto> Photos,
        List<VenueReviewDto> Reviews,
        decimal WalletBalance,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record VenueOwnerDto(
        Guid UserId,
        string? FullName,
        string? Phone,
        string? AvatarUrl,
        string? Bio,
        DateTime? DateOfBirth,
        string? Gender,
        string Role,
        string? PreferredPositions,
        int? ShirtNumber,
        string? PreferredFoot,
        bool IsActive
    );

    public record VenueFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal? PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record IgnoredHolidayDto(
        Guid Id,
        string Name,
        DateTime Date
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

    public record VenueReviewDto(
        Guid ReviewId,
        Guid UserId,
        string? UserFullName,
        string? UserAvatarUrl,
        int Rating,
        string? Comment,
        string? OwnerResponse,
        DateTime? ResponseDate,
        DateTime CreatedAt
    );
}
