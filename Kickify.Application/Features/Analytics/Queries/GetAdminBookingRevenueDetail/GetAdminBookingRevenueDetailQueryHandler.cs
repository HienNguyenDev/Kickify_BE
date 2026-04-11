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

        var response = new GetAdminBookingRevenueDetailResponse(
            row.BookingId,
            row.RoomId,
            venue.VenueId,
            venue.VenueName,
            venue.Address,
            row.FieldId,
            row.Field.FieldName,
            row.Field.FieldType,
            row.TotalAmount,
            row.PlatformFee,
            row.VenueAmount,
            row.Status,
            room.Status,
            row.BookingDate,
            row.StartTime,
            row.EndTime,
            room.UpdatedAt,
            row.PaymentMethod,
            row.TransactionReference,
            room.HostId,
            room.Host.FullName ?? room.Host.Email
        );

        return Result.Success(response);
    }
}
