namespace Kickify.Application.Features.Analytics.Queries.GetVenueDashboard;

// ── Summary ──
public record VenueDashboardSummaryDto(
    decimal TotalRevenue,
    int TotalBookings,
    decimal? AvgRating,
    int ActiveVenuesCount
);

// ── Revenue chart ──
public record RevenueSeriesItemDto(
    string Date,
    decimal Revenue
);

// ── Bookings chart ──
public record BookingsSeriesItemDto(
    string Date,
    int TotalBookings,
    int Confirmed,
    int Pending,
    int Cancelled
);

// ── Upcoming booking list ──
public record UpcomingBookingDto(
    Guid BookingId,
    Guid VenueId,
    string VenueName,
    string? CustomerName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    decimal Amount
);

// ── Recent review list ──
public record RecentReviewDto(
    Guid ReviewId,
    Guid VenueId,
    string VenueName,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    bool HasReply
);

// ── Root response ──
public record GetVenueDashboardResponse(
    VenueDashboardSummaryDto Summary,
    List<RevenueSeriesItemDto> RevenueSeries,
    List<BookingsSeriesItemDto> BookingsSeries,
    List<UpcomingBookingDto> UpcomingBookings,
    List<RecentReviewDto> RecentReviews
);
