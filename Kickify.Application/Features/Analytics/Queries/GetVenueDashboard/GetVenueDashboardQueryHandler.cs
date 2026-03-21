using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetVenueDashboard;

public class GetVenueDashboardQueryHandler
    : IQueryHandler<GetVenueDashboardQuery, GetVenueDashboardResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    public GetVenueDashboardQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetVenueDashboardResponse>> Handle(
        GetVenueDashboardQuery request, CancellationToken cancellationToken)
    {
        // ── 1. Resolve timezone ──
        var tz = ResolveTimezone(request.Timezone);

        // ── 2. Compute UTC boundaries ──
        var fromLocal = request.FromDate.Date;
        var toLocalNextDay = request.ToDate.Date.AddDays(1);

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(fromLocal, DateTimeKind.Unspecified), tz);
        var toUtcExclusive = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(toLocalNextDay, DateTimeKind.Unspecified), tz);

        // ── 3. Base booking query (venue-scoped) ──
        var bookingsQuery = _db.Bookings
            .Include(b => b.Field)
                .ThenInclude(f => f.Venue)
            .Include(b => b.MatchRoom)
                .ThenInclude(mr => mr.Host)
            .AsNoTracking()
            .Where(b => b.BookingDate >= fromUtc && b.BookingDate < toUtcExclusive);

        if (request.VenueId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Field.VenueId == request.VenueId.Value);
        }

        var bookings = await bookingsQuery.ToListAsync(cancellationToken);

        // ── 4. Revenue from wallet transactions ──
        var venueFieldIds = bookings.Select(b => b.FieldId).Distinct().ToList();
        var venueIds = bookings.Select(b => b.Field.VenueId).Distinct().ToList();

        // Get wallet ids for venue owners
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

        // ── 5. Summary ──
        var totalBookings = bookings.Count;

        // Avg rating from reviews within scope
        var reviewsQuery = _db.VenueReviews
            .AsNoTracking()
            .Where(r => r.CreatedAt >= fromUtc && r.CreatedAt < toUtcExclusive);

        if (request.VenueId.HasValue)
            reviewsQuery = reviewsQuery.Where(r => r.VenueId == request.VenueId.Value);
        else if (venueIds.Any())
            reviewsQuery = reviewsQuery.Where(r => venueIds.Contains(r.VenueId));

        var avgRating = await reviewsQuery.AnyAsync(cancellationToken)
            ? Math.Round((decimal)await reviewsQuery.AverageAsync(r => r.Rating, cancellationToken), 1)
            : (decimal?)null;

        // Active venues: venues with >= 1 booking in period
        var activeVenuesCount = bookings.Select(b => b.Field.VenueId).Distinct().Count();

        var summary = new VenueDashboardSummaryDto(totalRevenue, totalBookings, avgRating, activeVenuesCount);

        // ── 6. Revenue series (day buckets) ──
        var revenueSeries = new List<RevenueSeriesItemDto>();
        for (var day = fromLocal; day <= request.ToDate.Date; day = day.AddDays(1))
        {
            var dayStart = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(day, DateTimeKind.Unspecified), tz);
            var dayEnd = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(day.AddDays(1), DateTimeKind.Unspecified), tz);

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

        // ── 7. Bookings series (day buckets) ──
        var bookingsSeries = new List<BookingsSeriesItemDto>();
        for (var day = fromLocal; day <= request.ToDate.Date; day = day.AddDays(1))
        {
            var dayStart = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(day, DateTimeKind.Unspecified), tz);
            var dayEnd = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(day.AddDays(1), DateTimeKind.Unspecified), tz);

            var dayBookings = bookings
                .Where(b => b.BookingDate >= dayStart && b.BookingDate < dayEnd)
                .ToList();

            var confirmed = dayBookings.Count(b =>
                b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

            // Pending = bookings whose related payment is pending
            // Since we don't have a direct payment status on Booking, we count bookings that are Confirmed 
            // but we derive pending from PaymentRequests
            var dayBookingIds = dayBookings.Select(b => b.BookingId).ToList();
            var pendingPaymentBookingIds = await _db.PaymentRequests
                .Where(pr => pr.Status == PaymentStatus.Pending
                    && dayBookingIds.Any())
                .Select(pr => pr.PaymentRequestId)
                .CountAsync(cancellationToken);

            var cancelled = dayBookings.Count(b => b.Status == BookingStatus.Cancelled);

            // For pending, we use 0 as default since booking status doesn't have Pending
            bookingsSeries.Add(new BookingsSeriesItemDto(
                day.ToString("yyyy-MM-dd"),
                dayBookings.Count,
                confirmed,
                0, // Pending - no direct pending booking status in enum
                cancelled));
        }

        // ── 8. Upcoming bookings ──
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var nowUtc = DateTime.UtcNow;

        var upcomingQuery = _db.Bookings
            .Include(b => b.Field)
                .ThenInclude(f => f.Venue)
            .Include(b => b.MatchRoom)
                .ThenInclude(mr => mr.Host)
            .AsNoTracking()
            .Where(b => b.BookingDate >= nowUtc.Date
                && b.Status == BookingStatus.Confirmed);

        if (request.VenueId.HasValue)
            upcomingQuery = upcomingQuery.Where(b => b.Field.VenueId == request.VenueId.Value);

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

        // ── 9. Recent reviews ──
        var recentReviewsQuery = _db.VenueReviews
            .Include(r => r.Venue)
            .AsNoTracking();

        if (request.VenueId.HasValue)
            recentReviewsQuery = recentReviewsQuery.Where(r => r.VenueId == request.VenueId.Value);

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
