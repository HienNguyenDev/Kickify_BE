using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Analytics;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueDetail;

public class GetAdminBookingRevenueDetailQueryHandler
    : IQueryHandler<GetAdminBookingRevenueDetailQuery, GetAdminBookingRevenueDetailResponse>
{
    private readonly IApplicationDbContext _db;

    public GetAdminBookingRevenueDetailQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Match creation treats <see cref="Kickify.Domain.Entities.Booking.BookingDate"/> + StartTime as Vietnam local wall clock.
    /// </summary>
    private static TimeZoneInfo VietnamTimeZone { get; } = ResolveVietnamTimeZone();

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }

    /// <summary>Instant when the booked slot starts, converted to UTC (same convention as match creation).</summary>
    private static DateTime GetBookingSlotStartUtc(DateTime bookingDate, TimeSpan startTime)
    {
        var localUnspecified = DateTime.SpecifyKind(bookingDate.Date.Add(startTime), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localUnspecified, VietnamTimeZone);
    }

    /// <summary>
    /// DB timestamps are stored as UTC; normalize so JSON always serializes with consistent ISO-8601 UTC (Z).
    /// </summary>
    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    public async Task<Result<GetAdminBookingRevenueDetailResponse>> Handle(
        GetAdminBookingRevenueDetailQuery request, CancellationToken cancellationToken)
    {
        var row = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Field)
                .ThenInclude(f => f!.Venue)
            .Include(b => b.MatchRoom)
                .ThenInclude(m => m!.Host)
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

        if (row is null)
            return Result.Failure<GetAdminBookingRevenueDetailResponse>(
                BookingErrors.NotFound(request.BookingId));

        if (!CompletedBookingRevenue.IsCompletedRevenueBooking(row))
            return Result.Failure<GetAdminBookingRevenueDetailResponse>(
                BookingErrors.NotEligibleForRevenueReport(request.BookingId));

        var venue = row.Field.Venue;
        var room = row.MatchRoom;

        var bookingSlotStartUtc = GetBookingSlotStartUtc(row.BookingDate, row.StartTime);
        var matchCompletedAtUtc = EnsureUtc(room.UpdatedAt);

        // Revenue recognition uses room.UpdatedAt; it should not precede the scheduled slot start.
        if (matchCompletedAtUtc < bookingSlotStartUtc)
            matchCompletedAtUtc = bookingSlotStartUtc;

        var response = new GetAdminBookingRevenueDetailResponse(
            row.BookingId,
            row.RoomId,
            venue.VenueId,
            venue.VenueName,
            venue.Address,
            row.FieldId,
            row.Field.FieldName,
            row.Field.FieldType.ToString(),
            row.TotalAmount,
            row.PlatformFee,
            row.VenueAmount,
            row.Status.ToString(),
            room.Status.ToString(),
            bookingSlotStartUtc,
            row.StartTime,
            row.EndTime,
            matchCompletedAtUtc,
            row.PaymentMethod,
            row.TransactionReference,
            room.HostId,
            room.Host.FullName ?? room.Host.Email
        );

        return Result.Success(response);
    }
}
