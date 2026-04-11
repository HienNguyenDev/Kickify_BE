using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Features.Analytics;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

public class GetAdminBookingRevenueListQueryHandler
    : IQueryHandler<GetAdminBookingRevenueListQuery, GetAdminBookingRevenueListResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    public GetAdminBookingRevenueListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetAdminBookingRevenueListResponse>> Handle(
        GetAdminBookingRevenueListQuery request, CancellationToken cancellationToken)
    {
        var tz = ResolveTimezone(request.Timezone);
        var fromLocal = request.FromDate.Date;
        var toLocalNextDay = request.ToDate.Date.AddDays(1);

        var fromUtc = ToUtcBoundary(fromLocal, tz);
        var toUtcExclusive = ToUtcBoundary(toLocalNextDay, tz);

        var baseQuery = _db.Bookings
            .AsNoTracking()
            .WhereRecognizedBetween(fromUtc, toUtcExclusive);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(b => b.MatchRoom.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new AdminBookingRevenueListItemDto(
                b.BookingId,
                b.RoomId,
                b.Field.VenueId,
                b.Field.Venue.VenueName,
                b.Field.FieldName,
                b.TotalAmount,
                b.PlatformFee,
                b.VenueAmount,
                b.MatchRoom.UpdatedAt,
                b.Status,
                b.MatchRoom.Status))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetAdminBookingRevenueListResponse(
            items, totalCount, request.Page, request.PageSize));
    }

    private static DateTime ToUtcBoundary(DateTime localDate, TimeZoneInfo tz) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified), tz);

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
