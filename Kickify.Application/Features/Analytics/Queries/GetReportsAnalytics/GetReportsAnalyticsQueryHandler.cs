using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Analytics;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetReportsAnalytics;

public class GetReportsAnalyticsQueryHandler
    : IQueryHandler<GetReportsAnalyticsQuery, GetReportsAnalyticsResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    // Fixed Elo distribution ranges
    private static readonly (int Min, int Max, string Label)[] EloRanges =
    [
        (0, 999, "0-999"),
        (1000, 1199, "1000-1199"),
        (1200, 1399, "1200-1399"),
        (1400, 1599, "1400-1599"),
        (1600, 1799, "1600-1799"),
        (1800, int.MaxValue, "1800+")
    ];

    public GetReportsAnalyticsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetReportsAnalyticsResponse>> Handle(
        GetReportsAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var tz = ResolveTimezone(request.Timezone);
        var fromLocal = request.FromDate.Date;
        var toLocal = request.ToDate.Date;
        var toLocalNextDay = toLocal.AddDays(1);

        var fromUtc = ToUtc(fromLocal, tz);
        var toUtcExclusive = ToUtc(toLocalNextDay, tz);

        // Previous period of equal length
        var periodDays = (toLocal - fromLocal).Days + 1;
        var prevFromLocal = fromLocal.AddDays(-periodDays);
        var prevToLocalNextDay = fromLocal; // exclusive
        var prevFromUtc = ToUtc(prevFromLocal, tz);
        var prevToUtcExclusive = ToUtc(prevToLocalNextDay, tz);

        // ════════════════════════════════════════════
        // Summary: Total Users
        // ════════════════════════════════════════════
        var totalUsers = await _db.Users
            .CountAsync(u => u.DeletedAt == null, cancellationToken);
        var totalUsersPrev = await _db.Users
            .CountAsync(u => u.DeletedAt == null && u.CreatedAt < prevToUtcExclusive, cancellationToken);
        var totalUsersChangePct = CalcChangePct(totalUsers, totalUsersPrev);

        // ════════════════════════════════════════════
        // Summary: Active Users 30d
        // ════════════════════════════════════════════
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);

        var activeUsers30d = await _db.SystemLogs
            .Where(l => l.CreatedAt >= thirtyDaysAgo && l.CreatedAt <= now && l.UserId != null)
            .Select(l => l.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var activeUsersPrev30d = await _db.SystemLogs
            .Where(l => l.CreatedAt >= sixtyDaysAgo && l.CreatedAt < thirtyDaysAgo && l.UserId != null)
            .Select(l => l.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var activeUsers30dChangePct = CalcChangePct(activeUsers30d, activeUsersPrev30d);

        // ════════════════════════════════════════════
        // Summary: Total Bookings
        // ════════════════════════════════════════════
        var totalBookings = await _db.Bookings
            .CountAsync(b => b.BookingDate >= fromUtc && b.BookingDate < toUtcExclusive,
                cancellationToken);
        var totalBookingsPrev = await _db.Bookings
            .CountAsync(b => b.BookingDate >= prevFromUtc && b.BookingDate < prevToUtcExclusive,
                cancellationToken);
        var totalBookingsChangePct = CalcChangePctOrZero(totalBookings, totalBookingsPrev);

        // ════════════════════════════════════════════
        // Summary: Monthly Platform Fee Revenue
        // ════════════════════════════════════════════
        var currentMonthStart = new DateTime(toLocal.Year, toLocal.Month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1);
        var prevMonthStart = currentMonthStart.AddMonths(-1);

        var currentMonthStartUtc = ToUtc(currentMonthStart, tz);
        var currentMonthEndUtc = ToUtc(currentMonthEnd, tz);
        var prevMonthStartUtc = ToUtc(prevMonthStart, tz);

        var platformFeeTypes = new[] { TransactionType.BookingCommission, TransactionType.WithdrawalFee, TransactionType.PremiumPurchase };

        var monthlyRevenue = await _db.WalletTransactions
            .Where(t => platformFeeTypes.Contains(t.TransactionType) && t.Amount < 0
                && t.CreatedAt >= currentMonthStartUtc && t.CreatedAt < currentMonthEndUtc)
            .SumAsync(t => -t.Amount, cancellationToken);
        var prevMonthlyRevenue = await _db.WalletTransactions
            .Where(t => platformFeeTypes.Contains(t.TransactionType) && t.Amount < 0
                && t.CreatedAt >= prevMonthStartUtc && t.CreatedAt < currentMonthStartUtc)
            .SumAsync(t => -t.Amount, cancellationToken);
        var monthlyRevenueChangePct = CalcChangePct(monthlyRevenue, prevMonthlyRevenue);

        var summary = new ReportsAnalyticsSummaryDto(
            totalUsers, totalUsersChangePct,
            activeUsers30d, activeUsers30dChangePct,
            totalBookings, totalBookingsChangePct,
            monthlyRevenue, monthlyRevenueChangePct);

        // ════════════════════════════════════════════
        // User Analytics Tab: User Growth by Month
        // ════════════════════════════════════════════
        var allUsersInPeriod = await _db.Users
            .Where(u => u.DeletedAt == null && u.CreatedAt >= fromUtc && u.CreatedAt < toUtcExclusive)
            .Select(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        var allLogsInPeriod = await _db.SystemLogs
            .Where(l => l.CreatedAt >= fromUtc && l.CreatedAt < toUtcExclusive && l.UserId != null)
            .Select(l => new { l.UserId, l.CreatedAt })
            .ToListAsync(cancellationToken);

        var userGrowth = new List<UserGrowthMonthDto>();
        var monthCursor = new DateTime(fromLocal.Year, fromLocal.Month, 1);
        var endMonth = new DateTime(toLocal.Year, toLocal.Month, 1);

        while (monthCursor <= endMonth)
        {
            var mStart = ToUtc(monthCursor, tz);
            var mEnd = ToUtc(monthCursor.AddMonths(1), tz);

            // New users created in this month (non-cumulative per spec)
            var newUsers = allUsersInPeriod.Count(d => d >= mStart && d < mEnd);

            // Active users in this month
            var activeInMonth = allLogsInPeriod
                .Where(l => l.CreatedAt >= mStart && l.CreatedAt < mEnd)
                .Select(l => l.UserId)
                .Distinct()
                .Count();

            userGrowth.Add(new UserGrowthMonthDto(
                monthCursor.ToString("yyyy-MM"), newUsers, activeInMonth));

            monthCursor = monthCursor.AddMonths(1);
        }

        // ════════════════════════════════════════════
        // User Analytics Tab: User Distribution by Role
        // ════════════════════════════════════════════
        var playerCount = await _db.Users
            .CountAsync(u => u.DeletedAt == null && u.Role == UserRole.Player, cancellationToken);
        var venueOwnerCount = await _db.Users
            .CountAsync(u => u.DeletedAt == null && u.Role == UserRole.VenueOwner, cancellationToken);

        var userDistribution = new List<UserDistributionDto>
        {
            new("Player", playerCount),
            new("Venue Owners", venueOwnerCount)
        };

        // ════════════════════════════════════════════
        // Booking Reports Tab: Bookings by Venue Type (FieldType)
        // ════════════════════════════════════════════
        var bookingsByVenueType = await _db.Bookings
            .Where(b => b.BookingDate >= fromUtc && b.BookingDate < toUtcExclusive)
            .GroupBy(b => b.Field.FieldType)
            .Select(g => new BookingsByVenueTypeDto(
                g.Key.ToString(),
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        // Ensure all field types are represented
        foreach (var ft in Enum.GetValues<FieldType>())
        {
            if (!bookingsByVenueType.Any(x => x.Type == ft.ToString()))
            {
                bookingsByVenueType.Add(new BookingsByVenueTypeDto(ft.ToString(), 0));
            }
        }
        bookingsByVenueType = bookingsByVenueType.OrderBy(x => x.Type).ToList();

        // ════════════════════════════════════════════
        // Revenue Analysis Tab: Monthly Platform Fee Revenue & Bookings
        // ════════════════════════════════════════════
        var platformFeeRevenueRows = await _db.WalletTransactions
            .AsNoTracking()
            .Where(t => platformFeeTypes.Contains(t.TransactionType) && t.Amount < 0
                && t.CreatedAt >= fromUtc && t.CreatedAt < toUtcExclusive)
            .Select(t => new { t.Amount, t.CreatedAt })
            .ToListAsync(cancellationToken);

        // Count confirmed/completed bookings per month for context
        var bookingDatesInPeriod = await _db.Bookings
            .AsNoTracking()
            .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                && b.BookingDate >= fromUtc && b.BookingDate < toUtcExclusive)
            .Select(b => b.BookingDate)
            .ToListAsync(cancellationToken);

        var monthlyRevenueAndBookings = new List<MonthlyRevenueAndBookingsDto>();
        var revMonthCursor = new DateTime(fromLocal.Year, fromLocal.Month, 1);

        while (revMonthCursor <= endMonth)
        {
            var mStart = ToUtc(revMonthCursor, tz);
            var mEnd = ToUtc(revMonthCursor.AddMonths(1), tz);

            var revenueInMonth = platformFeeRevenueRows
                .Where(x => x.CreatedAt >= mStart && x.CreatedAt < mEnd)
                .Sum(x => -x.Amount);
            var bookingsInMonth = bookingDatesInPeriod
                .Count(d => d >= mStart && d < mEnd);

            monthlyRevenueAndBookings.Add(new MonthlyRevenueAndBookingsDto(
                revMonthCursor.ToString("yyyy-MM"), revenueInMonth, bookingsInMonth));

            revMonthCursor = revMonthCursor.AddMonths(1);
        }

        // ════════════════════════════════════════════
        // Elo Statistics Tab
        // ════════════════════════════════════════════
        var eloValues = await _db.PlayerProfiles
            .Select(p => p.CurrentElo)
            .ToListAsync(cancellationToken);

        var eloDistribution = EloRanges.Select(r => new EloDistributionDto(
            r.Label,
            eloValues.Count(e => e >= r.Min && e <= r.Max)
        )).ToList();

        return Result.Success(new GetReportsAnalyticsResponse(
            summary,
            userGrowth,
            userDistribution,
            bookingsByVenueType,
            monthlyRevenueAndBookings,
            eloDistribution
        ));
    }

    // ── Helpers ──

    private static double? CalcChangePct(decimal current, decimal previous)
    {
        if (previous == 0 && current == 0) return 0;
        if (previous == 0) return null;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }

    private static double? CalcChangePct(int current, int previous)
        => CalcChangePct((decimal)current, (decimal)previous);

    /// <summary>Like <see cref="CalcChangePct"/> but returns 0 when previous is 0 (API contract: non-null).</summary>
    private static double CalcChangePctOrZero(int current, int previous)
    {
        if (previous == 0) return 0;
        return Math.Round((double)(current - previous) / previous * 100, 1);
    }

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
