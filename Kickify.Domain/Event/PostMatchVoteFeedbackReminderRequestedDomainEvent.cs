using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Hangfire: remind players to vote and leave feedback (attempt 1 ≈ 15m, 2 ≈ 30m after match end).</summary>
public sealed record PostMatchVoteFeedbackReminderRequestedDomainEvent(
    Guid RoomId,
    int Attempt) : IDomainEvent;
