using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics;

/// <summary>
/// Revenue KPIs use field bookings whose match room has finished the lifecycle (<see cref="RoomStatus.Completed"/>).
/// Recognition time is <see cref="MatchRoom.UpdatedAt"/> when status transitions to Completed (escrow already released at match start).
/// </summary>
public static class CompletedBookingRevenue
{
    public static bool IsCompletedRevenueBooking(Booking b) =>
        (b.MatchRoom.Status == RoomStatus.Completed || b.MatchRoom.Status == RoomStatus.Reviewing)
        && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed);

    public static IQueryable<Booking> WhereCompletedRevenue(this IQueryable<Booking> bookings) =>
        bookings.Where(b =>
            b.MatchRoom.Status == RoomStatus.Completed
            && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed));

    /// <summary>Completed in [fromUtc, toUtcExclusive) by match room completion timestamp.</summary>
    public static IQueryable<Booking> WhereRecognizedBetween(
        this IQueryable<Booking> bookings,
        DateTime fromUtc,
        DateTime toUtcExclusive) =>
        bookings.WhereCompletedRevenue()
            .Where(b => b.MatchRoom.UpdatedAt >= fromUtc && b.MatchRoom.UpdatedAt < toUtcExclusive);

    public static Task<decimal> SumTotalAmountAsync(
        IQueryable<Booking> bookings,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken cancellationToken) =>
        bookings.WhereRecognizedBetween(fromUtc, toUtcExclusive)
            .SumAsync(b => b.TotalAmount, cancellationToken);

    // ── Paid revenue: all Confirmed/Completed bookings by BookingDate ──
    // Venue dashboard counts revenue via BookingIncome wallet transactions which are
    // credited at booking confirmation, not at match completion. Admin must use the same
    // wider definition so admin total >= any individual venue-owner total.

    /// <summary>
    /// All paid (Confirmed or Completed) bookings whose <see cref="Booking.BookingDate"/> falls in
    /// <c>[fromInclusive, toExclusive)</c>. For PostgreSQL <c>date</c> columns, pass calendar midnights
    /// (<see cref="DateTimeKind.Unspecified"/>) in the business timezone; UTC instants also work but
    /// can miscount against <c>date</c> when translated by the driver.
    /// </summary>
    public static IQueryable<Booking> WherePaidBetween(
        this IQueryable<Booking> bookings,
        DateTime fromInclusive,
        DateTime toExclusive) =>
        bookings.Where(b =>
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            && b.BookingDate >= fromInclusive && b.BookingDate < toExclusive);

    public static Task<decimal> SumPaidAmountAsync(
        IQueryable<Booking> bookings,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken) =>
        bookings.WherePaidBetween(fromInclusive, toExclusive)
            .SumAsync(b => b.TotalAmount, cancellationToken);
}
