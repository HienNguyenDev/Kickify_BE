namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogs;

public record GetSystemLogsResponse(
    IReadOnlyList<SystemLogListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record SystemLogListItemDto(
    Guid LogId,
    Guid? UserId,
    string? UserName,
    string Action,
    string? EntityType,
    Guid? EntityId,
    string? UserAgent,
    string ResponseStatus,
    string? ErrorMessage,
    DateTime CreatedAtUtc
);
