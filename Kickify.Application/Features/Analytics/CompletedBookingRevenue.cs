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
        b.MatchRoom.Status == RoomStatus.Completed
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
}
