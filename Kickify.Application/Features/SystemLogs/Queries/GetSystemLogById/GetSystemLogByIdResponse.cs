namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogById;

public record GetSystemLogByIdResponse(
    Guid LogId,
    Guid? UserId,
    string? UserName,
    string? UserEmail,
    string Action,
    string? EntityType,
    Guid? EntityId,
    string? UserAgent,
    string ResponseStatus,
    string? ErrorMessage,
    DateTime CreatedAtUtc
);
