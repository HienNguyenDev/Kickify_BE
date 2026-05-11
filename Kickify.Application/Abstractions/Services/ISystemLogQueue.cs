using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Services;

public interface ISystemLogQueue
{
    bool TryEnqueue(SystemLogQueueItem item);
}

public sealed record SystemLogQueueItem(
    Guid? UserId,
    string? UserName,
    SystemLogAction Action,
    string? EntityType,
    Guid? EntityId,
    string? UserAgent,
    SystemLogResponseStatus ResponseStatus,
    string? ErrorMessage,
    DateTime CreatedAtUtc);
