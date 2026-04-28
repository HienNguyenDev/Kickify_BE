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
        List<FieldSummaryDto> Fields,
        string? PrimaryPhotoUrl,
        DateTime CreatedAt
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

    public record FieldSummaryDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        List<FieldPeakHourResponseDto> PeakHours,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record FieldPeakHourResponseDto(
        Guid Id,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
    );
}
