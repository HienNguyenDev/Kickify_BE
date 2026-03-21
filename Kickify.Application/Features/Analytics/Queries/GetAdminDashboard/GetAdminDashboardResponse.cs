namespace Kickify.Application.Features.Analytics.Queries.GetAdminDashboard;

// ── KPI cards ──
public record AdminKpiDto(
    int ActiveUsersToday,
    double? ActiveUsersTodayChangePct,
    int MatchesToday,
    double? MatchesTodayChangePct,
    int PendingReports,
    int PendingReportsChangeAbs,
    decimal Revenue30d,
    double? Revenue30dChangePct
);

// ── User growth chart ──
public record UserGrowthItemDto(
    string Day,
    int Users,
    int Prev
);

// ── Matches by day chart ──
public record MatchesByDayItemDto(
    string Day,
    int Matches
);

// ── Revenue trend chart ──
public record RevenueTrendItemDto(
    string Date,
    decimal Revenue
);

// ── System alerts ──
public record SystemAlertDto(
    Guid Id,
    string Severity,
    string Message,
    DateTime CreatedAt
);

// ── Today matches ──
public record TodayMatchDto(
    Guid MatchId,
    string TeamsLabel,
    string? VenueName,
    DateTime StartTime,
    string Status
);

// ── Root response ──
public record GetAdminDashboardResponse(
    AdminKpiDto Kpi,
    List<UserGrowthItemDto> UserGrowth,
    List<MatchesByDayItemDto> MatchesByDay,
    List<RevenueTrendItemDto> RevenueTrend,
    List<SystemAlertDto> SystemAlerts,
    List<TodayMatchDto> TodayMatches
);
