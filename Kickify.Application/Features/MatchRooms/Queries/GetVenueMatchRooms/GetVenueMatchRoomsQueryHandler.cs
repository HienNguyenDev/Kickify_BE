using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.MatchRooms.Queries.GetVenueMatchRooms;

public class GetVenueMatchRoomsQueryHandler : IQueryHandler<GetVenueMatchRoomsQuery, GetVenueMatchRoomsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IUserContext _userContext;

    public GetVenueMatchRoomsQueryHandler(IApplicationDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<Result<GetVenueMatchRoomsResponse>> Handle(
        GetVenueMatchRoomsQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _userContext.UserId;

        var query = _db.MatchRooms
            .AsNoTracking()
            .Include(r => r.Host)
            .Include(r => r.Field)
                .ThenInclude(f => f!.Venue)
            .Include(r => r.Booking)
            .Where(r => r.FieldId != null && r.Field!.Venue.OwnerId == ownerId);

        // Optional: filter by specific venue
        if (request.VenueId.HasValue)
            query = query.Where(r => r.Field!.VenueId == request.VenueId.Value);

        // Optional: filter by specific field
        if (request.FieldId.HasValue)
            query = query.Where(r => r.FieldId == request.FieldId.Value);

        // Optional: filter by match date
        if (request.Date.HasValue)
            query = query.Where(r => r.MatchDate.Date == request.Date.Value.Date);

        // Optional: filter by room status
        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<RoomStatus>(request.Status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        var total = await query.CountAsync(cancellationToken);

        var rooms = await query
            .OrderByDescending(r => r.MatchDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = rooms.Select(r =>
        {
            var endTime = r.StartTime.Add(TimeSpan.FromMinutes(r.DurationMinutes));
            return new VenueMatchRoomItemDto(
                r.RoomId,
                r.RoomName,
                r.FieldId,
                r.Field?.FieldName,
                r.Field?.Venue.VenueId,
                r.Field?.Venue.VenueName,
                r.Host.FullName ?? "Unknown",
                r.Host.AvatarUrl,
                r.MatchDate,
                r.StartTime,
                endTime,
                r.DurationMinutes,
                r.MatchFormat.ToString(),
                r.TotalSlots,
                r.FilledSlots,
                r.DepositPerPerson,
                r.TotalDepositCollected,
                r.Status.ToString(),
                r.Booking?.Status.ToString(),
                r.Booking?.TotalAmount,
                r.FinalResult?.ToString(),
                r.CreatedAt
            );
        }).ToList();

        var response = new GetVenueMatchRoomsResponse(
            items,
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize)
        );

        return Result.Success(response);
    }
}
