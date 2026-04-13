using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogs;

public class GetSystemLogsQueryHandler
    : IQueryHandler<GetSystemLogsQuery, GetSystemLogsResponse>
{
    private readonly IApplicationDbContext _db;
    private const string DefaultTimezone = "Asia/Ho_Chi_Minh";

    public GetSystemLogsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetSystemLogsResponse>> Handle(
        GetSystemLogsQuery request, CancellationToken cancellationToken)
    {
        var tz = ResolveTimezone(request.Timezone);
        var fromLocal = request.FromDate.Date;
        var toLocalNextDay = request.ToDate.Date.AddDays(1);

        var fromUtc = ToUtcStartOfDay(fromLocal, tz);
        var toUtcExclusive = ToUtcStartOfDay(toLocalNextDay, tz);

        var query = _db.SystemLogs
            .AsNoTracking()
            .Where(l => l.CreatedAt >= fromUtc && l.CreatedAt < toUtcExclusive);

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);

        if (request.Action.HasValue)
            query = query.Where(l => l.Action == request.Action.Value);

        if (request.ResponseStatus.HasValue)
            query = query.Where(l => l.ResponseStatus == request.ResponseStatus.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var term = request.EntityType.Trim().ToLowerInvariant();
            query = query.Where(l => l.EntityType != null && l.EntityType.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new SystemLogListItemDto(
                l.LogId,
                l.UserId,
                l.UserName,
                l.Action.ToString(),
                l.EntityType,
                l.EntityId,
                l.UserAgent,
                l.ResponseStatus.ToString(),
                l.ErrorMessage,
                l.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetSystemLogsResponse(
            items, totalCount, request.Page, request.PageSize));
    }

    private static DateTime ToUtcStartOfDay(DateTime localDate, TimeZoneInfo tz)
    {
        var unspecified = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
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
