using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Analytics;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminDashboard;

public class GetAdminDashboardQueryHandler
    : IQueryHandler<GetAdminDashboardQuery, GetAdminDashboardResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    public GetAdminDashboardQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetAdminDashboardResponse>> Handle(
        GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var tz = ResolveTimezone(request.Timezone);
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz).Date;
        var yesterdayLocal = todayLocal.AddDays(-1);

        var todayStartUtc = ToUtc(todayLocal, tz);
        var todayEndUtc = ToUtc(todayLocal.AddDays(1), tz);
        var yesterdayStartUtc = ToUtc(yesterdayLocal, tz);

        // ════════════════════════════════════════════
        // KPI: Active Users Today
        // ════════════════════════════════════════════
        // Active = has SystemLog entry (login/business action) on that day
        var activeUsersToday = await CountActiveUsers(todayStartUtc, todayEndUtc, cancellationToken);
        var activeUsersYesterday = await CountActiveUsers(yesterdayStartUtc, todayStartUtc, cancellationToken);
        var activeUsersTodayChangePct = CalcChangePct(activeUsersToday, activeUsersYesterday);

        // ════════════════════════════════════════════
        // KPI: Matches Today
        // ════════════════════════════════════════════
        var matchesToday = await _db.MatchRooms
            .CountAsync(m => m.MatchDate >= todayStartUtc && m.MatchDate < todayEndUtc,
                cancellationToken);
        var matchesYesterday = await _db.MatchRooms
            .CountAsync(m => m.MatchDate >= yesterdayStartUtc && m.MatchDate < todayStartUtc,
                cancellationToken);
        var matchesTodayChangePct = CalcChangePct(matchesToday, matchesYesterday);

        // ════════════════════════════════════════════
        // KPI: Pending Reports (player + content)
        // ════════════════════════════════════════════
        var pendingPlayerReports = await _db.PlayerReports
            .CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);
        var pendingContentReports = await _db.ContentReports
            .CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);
        var pendingReports = pendingPlayerReports + pendingContentReports;

        // Previous snapshot: reports that were pending as of yesterday
        // We approximate by counting reports created before today that are still pending
        var pendingReportsYesterday = await _db.PlayerReports
            .CountAsync(r => r.Status == ReportStatus.Pending && r.CreatedAt < todayStartUtc,
                cancellationToken)
            + await _db.ContentReports
            .CountAsync(r => r.Status == ReportStatus.Pending && r.CreatedAt < todayStartUtc,
                cancellationToken);
        var pendingReportsChangeAbs = pendingReports - pendingReportsYesterday;

        // ════════════════════════════════════════════
        // KPI: Revenue 30d
        // ════════════════════════════════════════════
        var thirtyDaysAgoLocal = todayLocal.AddDays(-30);
        var sixtyDaysAgoLocal = todayLocal.AddDays(-60);
        var thirtyDaysAgoUtc = ToUtc(thirtyDaysAgoLocal, tz);
        var sixtyDaysAgoUtc = ToUtc(sixtyDaysAgoLocal, tz);

        var revenue30d = await CompletedBookingRevenue.SumTotalAmountAsync(
            _db.Bookings, thirtyDaysAgoUtc, todayEndUtc, cancellationToken);
        var revenuePrev30d = await CompletedBookingRevenue.SumTotalAmountAsync(
            _db.Bookings, sixtyDaysAgoUtc, thirtyDaysAgoUtc, cancellationToken);
        var revenue30dChangePct = CalcChangePct(revenue30d, revenuePrev30d);

        var kpi = new AdminKpiDto(
            activeUsersToday, activeUsersTodayChangePct,
            matchesToday, matchesTodayChangePct,
            pendingReports, pendingReportsChangeAbs,
            revenue30d, revenue30dChangePct);

        // ════════════════════════════════════════════
        // Chart: User Growth (newly registered users by day, with previous period mirror)
        // ════════════════════════════════════════════
        var chartStartLocal = todayLocal.AddDays(-(request.ChartDays - 1));
        var prevChartStartLocal = chartStartLocal.AddDays(-request.ChartDays);

        var chartStartUtc = ToUtc(chartStartLocal, tz);
        var prevChartStartUtc = ToUtc(prevChartStartLocal, tz);

        // Use Users.CreatedAt as the stable source for growth.
        var allUsers = await _db.Users
            .Where(u => u.CreatedAt >= prevChartStartUtc && u.CreatedAt < todayEndUtc)
            .Select(u => new { u.UserId, u.CreatedAt })
            .ToListAsync(cancellationToken);

        var userGrowth = new List<UserGrowthItemDto>();
        for (int i = 0; i < request.ChartDays; i++)
        {
            var day = chartStartLocal.AddDays(i);
            var dayStart = ToUtc(day, tz);
            var dayEnd = ToUtc(day.AddDays(1), tz);

            var users = allUsers
                .Where(l => l.CreatedAt >= dayStart && l.CreatedAt < dayEnd)
                .Select(l => (Guid?)l.UserId)
                .Distinct()
                .Count();

            // Mirror day from previous period
            var prevDay = day.AddDays(-request.ChartDays);
            var prevDayStart = ToUtc(prevDay, tz);
            var prevDayEnd = ToUtc(prevDay.AddDays(1), tz);

            var prev = allUsers
                .Where(l => l.CreatedAt >= prevDayStart && l.CreatedAt < prevDayEnd)
                .Select(l => (Guid?)l.UserId)
                .Distinct()
                .Count();

            userGrowth.Add(new UserGrowthItemDto(day.ToString("yyyy-MM-dd"), users, prev));
        }

        // ════════════════════════════════════════════
        // Chart: Matches by Day
        // ════════════════════════════════════════════
        var matchRooms = await _db.MatchRooms
            .Where(m => m.MatchDate >= chartStartUtc && m.MatchDate < todayEndUtc)
            .Select(m => m.MatchDate)
            .ToListAsync(cancellationToken);

        var matchesByDay = new List<MatchesByDayItemDto>();
        for (int i = 0; i < request.ChartDays; i++)
        {
            var day = chartStartLocal.AddDays(i);
            var dayStart = ToUtc(day, tz);
            var dayEnd = ToUtc(day.AddDays(1), tz);

            var count = matchRooms.Count(d => d >= dayStart && d < dayEnd);
            matchesByDay.Add(new MatchesByDayItemDto(day.ToString("yyyy-MM-dd"), count));
        }

        // ════════════════════════════════════════════
        // Chart: Revenue Trend (completed field bookings, by completion day in timezone)
        // ════════════════════════════════════════════
        var revenueRows = await _db.Bookings
            .AsNoTracking()
            .WhereRecognizedBetween(chartStartUtc, todayEndUtc)
            .Select(b => new { b.TotalAmount, CompletedAt = b.MatchRoom.UpdatedAt })
            .ToListAsync(cancellationToken);

        var revenueTrend = new List<RevenueTrendItemDto>();
        for (int i = 0; i < request.ChartDays; i++)
        {
            var day = chartStartLocal.AddDays(i);
            var dayRevenue = revenueRows
                .Where(r => ToLocalDateUtc(r.CompletedAt, tz) == day)
                .Sum(r => r.TotalAmount);

            revenueTrend.Add(new RevenueTrendItemDto(day.ToString("yyyy-MM-dd"), Math.Max(0m, dayRevenue)));
        }

        // ════════════════════════════════════════════
        // System Alerts (from Announcements)
        // ════════════════════════════════════════════
        var announcementEntities = await _db.Announcements
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.SystemAlertsPageSize)
            .ToListAsync(cancellationToken);

        var systemAlerts = announcementEntities.Select(a => new SystemAlertDto(
            a.AnnouncementId,
            MapSeverity(a.Priority),
            a.Content,
            a.CreatedAt
        )).ToList();

        // ════════════════════════════════════════════
        // Today Matches
        // ════════════════════════════════════════════
        var todayMatchEntities = await _db.MatchRooms
            .Include(m => m.Field)
                .ThenInclude(f => f!.Venue)
            .Where(m => m.MatchDate >= todayStartUtc && m.MatchDate < todayEndUtc)
            .OrderBy(m => m.StartTime)
            .Take(request.TodayMatchesPageSize)
            .ToListAsync(cancellationToken);

        var todayMatches = todayMatchEntities.Select(m => new TodayMatchDto(
            m.RoomId,
            (m.TeamAName ?? "Team A") + " vs " + (m.TeamBName ?? "Team B"),
            m.Field != null ? m.Field.Venue.VenueName : m.CustomLocation,
            m.MatchDate.Date.Add(m.StartTime),
            m.Status.ToString()
        )).ToList();

        return Result.Success(new GetAdminDashboardResponse(
            kpi, userGrowth, matchesByDay, revenueTrend, systemAlerts, todayMatches));
    }

    // ── Helpers ──

    private async Task<int> CountActiveUsers(
        DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct)
    {
        var logUsers = _db.SystemLogs
            .Where(l => l.CreatedAt >= fromUtc && l.CreatedAt < toUtcExclusive && l.UserId != null)
            .Select(l => l.UserId);

        var newUsers = _db.Users
            .Where(u => u.CreatedAt >= fromUtc && u.CreatedAt < toUtcExclusive)
            .Select(u => (Guid?)u.UserId);

        var feedbackReviewerUsers = _db.MatchFeedbacks
            .Where(f => f.CreatedAt >= fromUtc && f.CreatedAt < toUtcExclusive)
            .Select(f => (Guid?)f.ReviewerId);

        var feedbackRevieweeUsers = _db.MatchFeedbacks
            .Where(f => f.CreatedAt >= fromUtc && f.CreatedAt < toUtcExclusive)
            .Select(f => (Guid?)f.RevieweeId);

        var joinedRoomUsers = _db.RoomParticipants
            .Where(p => p.JoinDate >= fromUtc && p.JoinDate < toUtcExclusive)
            .Select(p => (Guid?)p.UserId);

        return await logUsers
            .Concat(newUsers)
            .Concat(feedbackReviewerUsers)
            .Concat(feedbackRevieweeUsers)
            .Concat(joinedRoomUsers)
            .Distinct()
            .CountAsync(ct);
    }

    private static DateTime ToLocalDateUtc(DateTime utcTimestamp, TimeZoneInfo tz)
    {
        var utc = utcTimestamp.Kind == DateTimeKind.Utc
            ? utcTimestamp
            : DateTime.SpecifyKind(utcTimestamp, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz).Date;
    }

    private static double? CalcChangePct(decimal current, decimal previous)
    {
        if (previous == 0 && current == 0) return 0;
        if (previous == 0) return null;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }

    private static double? CalcChangePct(int current, int previous)
        => CalcChangePct((decimal)current, (decimal)previous);

    private static string MapSeverity(Priority priority) => priority switch
    {
        Priority.Low => "info",
        Priority.Medium => "warning",
        Priority.High => "danger",
        _ => "info"
    };

    private static DateTime ToUtc(DateTime localDate, TimeZoneInfo tz)
    {
        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified), tz);
    }

    private static TimeZoneInfo ResolveTimezone(string? timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezone);
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezone);
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimezone);
        }
    }
}
