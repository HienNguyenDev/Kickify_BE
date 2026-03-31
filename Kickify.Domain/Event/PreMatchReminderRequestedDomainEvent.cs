using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Hangfire: send 60 or 30 minute pre-match reminders to all participants.</summary>
public sealed record PreMatchReminderRequestedDomainEvent(
    Guid RoomId,
    int MinutesBefore) : IDomainEvent;
