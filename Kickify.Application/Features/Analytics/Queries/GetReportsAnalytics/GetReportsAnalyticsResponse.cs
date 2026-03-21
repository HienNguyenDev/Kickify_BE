namespace Kickify.Application.Features.Analytics.Queries.GetReportsAnalytics;

// ── Summary KPIs ──
public record ReportsAnalyticsSummaryDto(
    int TotalUsers,
    double? TotalUsersChangePct,
    int ActiveUsers30d,
    double? ActiveUsers30dChangePct,
    int TotalBookings,
    double? TotalBookingsChangePct,
    decimal MonthlyRevenue,
    double? MonthlyRevenueChangePct
);

// ── User Analytics tab ──
public record UserGrowthMonthDto(
    string Month,
    int Users,
    int Active
);

public record UserDistributionDto(
    string Name,
    int Value
);

// ── Booking Reports tab ──
public record BookingsByVenueTypeDto(
    string Type,
    int Bookings
);

// ── Revenue Analysis tab ──
public record MonthlyRevenueAndBookingsDto(
    string Month,
    decimal Revenue,
    int Bookings
);

// ── Elo Statistics tab ──
public record EloDistributionDto(
    string Range,
    int Players
);

// ── Root response ──
public record GetReportsAnalyticsResponse(
    ReportsAnalyticsSummaryDto Summary,
    List<UserGrowthMonthDto> UserGrowth,
    List<UserDistributionDto> UserDistribution,
    List<BookingsByVenueTypeDto> BookingsByVenueType,
    List<MonthlyRevenueAndBookingsDto> MonthlyRevenueAndBookings,
    List<EloDistributionDto> EloDistribution
);
