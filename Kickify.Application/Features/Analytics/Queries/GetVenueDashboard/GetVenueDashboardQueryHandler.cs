using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetVenueDashboard;

public class GetVenueDashboardQueryHandler
    : IQueryHandler<GetVenueDashboardQuery, GetVenueDashboardResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUserContext _userContext;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    public GetVenueDashboardQueryHandler(IApplicationDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<Result<GetVenueDashboardResponse>> Handle(
        GetVenueDashboardQuery request, CancellationToken cancellationToken)
    {
        // ── 1. Resolve timezone ──
        var tz = ResolveTimezone(request.Timezone);

        // ── 2. Venue scope: Admin = all (optional venueId); VenueOwner = only owned venues ──
        List<Guid> ownedVenueIds = new();
        if (!request.IsAdmin)
        {
            ownedVenueIds = await _db.Venues
                .AsNoTracking()
                .Where(v => v.OwnerId == _userContext.UserId)
                .Select(v => v.VenueId)
                .ToListAsync(cancellationToken);

            if (request.VenueId.HasValue && !ownedVenueIds.Contains(request.VenueId.Value))
                return Result.Failure<GetVenueDashboardResponse>(VenueErrors.Unauthorized);
        }

        // ── 3. Compute UTC boundaries (Npgsql-safe for timestamp without time zone) ──
        var fromLocal = request.FromDate.Date;
        var toLocalNextDay = request.ToDate.Date.AddDays(1);

        var fromUtc = ToUtcBoundary(fromLocal, tz);
        var toUtcExclusive = ToUtcBoundary(toLocalNextDay, tz);

        // ── 4. Base booking query (venue-scoped) ──
        var bookingsQuery = _db.Bookings
            .Include(b => b.Field)
                .ThenInclude(f => f.Venue)
            .Include(b => b.MatchRoom)
                .ThenInclude(mr => mr.Host)
            .AsNoTracking()
            .Where(b => b.BookingDate >= fromUtc && b.BookingDate < toUtcExclusive);

        if (request.IsAdmin)
        {
            if (request.VenueId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.Field.VenueId == request.VenueId.Value);
        }
        else
        {
            if (!ownedVenueIds.Any())
                bookingsQuery = bookingsQuery.Where(_ => false);
            else if (request.VenueId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.Field.VenueId == request.VenueId.Value);
            else
                bookingsQuery = bookingsQuery.Where(b => ownedVenueIds.Contains(b.Field.VenueId));
        }

        var bookings = await bookingsQuery.ToListAsync(cancellationToken);

        // ── 5. Revenue from wallet transactions ──
        var venueIds = bookings.Select(b => b.Field.VenueId).Distinct().ToList();

        var venueOwnerIds = await _db.Venues
            .Where(v => venueIds.Contains(v.VenueId))
            .Select(v => v.OwnerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var walletIds = await _db.Wallets
            .Where(w => venueOwnerIds.Contains(w.UserId))
            .Select(w => w.WalletId)
            .ToListAsync(cancellationToken);

        var transactions = await _db.WalletTransactions
            .Where(t => walletIds.Contains(t.WalletId)
                && t.CreatedAt >= fromUtc && t.CreatedAt < toUtcExclusive
                && (t.TransactionType == TransactionType.BookingIncome
                    || t.TransactionType == TransactionType.Refund))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var paidAmount = transactions
            .Where(t => t.TransactionType == TransactionType.BookingIncome)
            .Sum(t => t.Amount);
        var refundedAmount = transactions
            .Where(t => t.TransactionType == TransactionType.Refund)
            .Sum(t => Math.Abs(t.Amount));

        var totalRevenue = paidAmount - refundedAmount;

        // ── 6. Summary ──
        var totalBookings = bookings.Count;

        var reviewsQuery = _db.VenueReviews
            .AsNoTracking()
            .Where(r => r.CreatedAt >= fromUtc && r.CreatedAt < toUtcExclusive);

        if (request.IsAdmin)
        {
            if (request.VenueId.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.VenueId == request.VenueId.Value);
            else if (venueIds.Any())
                reviewsQuery = reviewsQuery.Where(r => venueIds.Contains(r.VenueId));
        }
        else
        {
            if (request.VenueId.HasValue)
                reviewsQuery = reviewsQuery.Where(r => r.VenueId == request.VenueId.Value);
            else if (ownedVenueIds.Any())
                reviewsQuery = reviewsQuery.Where(r => ownedVenueIds.Contains(r.VenueId));
            else
                reviewsQuery = reviewsQuery.Where(_ => false);
        }

        var avgRating = await reviewsQuery.AnyAsync(cancellationToken)
            ? Math.Round((decimal)await reviewsQuery.AverageAsync(r => r.Rating, cancellationToken), 1)
            : (decimal?)null;

        var activeVenuesCount = bookings.Select(b => b.Field.VenueId).Distinct().Count();

        var summary = new VenueDashboardSummaryDto(totalRevenue, totalBookings, avgRating, activeVenuesCount);

        // ── 7. Revenue series (day buckets) ──
        var revenueSeries = new List<RevenueSeriesItemDto>();
        for (var day = fromLocal; day <= request.ToDate.Date; day = day.AddDays(1))
        {
            var dayStart = ToUtcBoundary(day, tz);
            var dayEnd = ToUtcBoundary(day.AddDays(1), tz);

            var dayPaid = transactions
                .Where(t => t.TransactionType == TransactionType.BookingIncome
                    && t.CreatedAt >= dayStart && t.CreatedAt < dayEnd)
                .Sum(t => t.Amount);
            var dayRefund = transactions
                .Where(t => t.TransactionType == TransactionType.Refund
                    && t.CreatedAt >= dayStart && t.CreatedAt < dayEnd)
                .Sum(t => Math.Abs(t.Amount));

            revenueSeries.Add(new RevenueSeriesItemDto(
                day.ToString("yyyy-MM-dd"),
                dayPaid - dayRefund));
        }

        // ── 8. Bookings series (day buckets) ──
        var bookingsSeries = new List<BookingsSeriesItemDto>();
        for (var day = fromLocal; day <= request.ToDate.Date; day = day.AddDays(1))
        {
            var dayStart = ToUtcBoundary(day, tz);
            var dayEnd = ToUtcBoundary(day.AddDays(1), tz);

            var dayBookings = bookings
                .Where(b => b.BookingDate >= dayStart && b.BookingDate < dayEnd)
                .ToList();

            var confirmed = dayBookings.Count(b =>
                b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

            var cancelled = dayBookings.Count(b => b.Status == BookingStatus.Cancelled);

            bookingsSeries.Add(new BookingsSeriesItemDto(
                day.ToString("yyyy-MM-dd"),
                dayBookings.Count,
                confirmed,
                0,
                cancelled));
        }

        // ── 9. Upcoming bookings ──
        // timestamptz columns require DateTimeKind.Utc (Npgsql); do not use Unspecified.
        var utcNow = DateTime.UtcNow;
        var todayUtcMidnight = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);

        var upcomingQuery = _db.Bookings
            .Include(b => b.Field)
                .ThenInclude(f => f.Venue)
            .Include(b => b.MatchRoom)
                .ThenInclude(mr => mr.Host)
            .AsNoTracking()
            .Where(b => b.BookingDate >= todayUtcMidnight
                && b.Status == BookingStatus.Confirmed);

        if (request.IsAdmin)
        {
            if (request.VenueId.HasValue)
                upcomingQuery = upcomingQuery.Where(b => b.Field.VenueId == request.VenueId.Value);
        }
        else
        {
            if (!ownedVenueIds.Any())
                upcomingQuery = upcomingQuery.Where(_ => false);
            else if (request.VenueId.HasValue)
                upcomingQuery = upcomingQuery.Where(b => b.Field.VenueId == request.VenueId.Value);
            else
                upcomingQuery = upcomingQuery.Where(b => ownedVenueIds.Contains(b.Field.VenueId));
        }

        var upcomingEntities = await upcomingQuery
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Take(request.UpcomingBookingsPageSize)
            .ToListAsync(cancellationToken);

        var upcomingBookings = upcomingEntities.Select(b => new UpcomingBookingDto(
            b.BookingId,
            b.Field.VenueId,
            b.Field.Venue.VenueName,
            b.MatchRoom?.Host?.FullName,
            b.BookingDate.Date.Add(b.StartTime),
            b.BookingDate.Date.Add(b.EndTime),
            b.Status.ToString(),
            b.TotalAmount
        )).ToList();

        // ── 10. Recent reviews ──
        var recentReviewsQuery = _db.VenueReviews
            .Include(r => r.Venue)
            .AsNoTracking();

        if (request.IsAdmin)
        {
            if (request.VenueId.HasValue)
                recentReviewsQuery = recentReviewsQuery.Where(r => r.VenueId == request.VenueId.Value);
        }
        else
        {
            if (!ownedVenueIds.Any())
                recentReviewsQuery = recentReviewsQuery.Where(_ => false);
            else if (request.VenueId.HasValue)
                recentReviewsQuery = recentReviewsQuery.Where(r => r.VenueId == request.VenueId.Value);
            else
                recentReviewsQuery = recentReviewsQuery.Where(r => ownedVenueIds.Contains(r.VenueId));
        }

        var recentReviewEntities = await recentReviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Take(request.RecentReviewsPageSize)
            .ToListAsync(cancellationToken);

        var recentReviews = recentReviewEntities.Select(r => new RecentReviewDto(
            r.ReviewId,
            r.VenueId,
            r.Venue.VenueName,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.OwnerResponse != null
        )).ToList();

        return Result.Success(new GetVenueDashboardResponse(
            summary,
            revenueSeries,
            bookingsSeries,
            upcomingBookings,
            recentReviews
        ));
    }

    private static DateTime ToUtcBoundary(DateTime localDate, TimeZoneInfo tz)
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
