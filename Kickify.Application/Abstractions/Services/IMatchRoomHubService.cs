namespace Kickify.Application.Abstractions.Services;

/// <summary>
/// Service for sending real-time notifications to match room participants
/// </summary>
public interface IMatchRoomHubService
{
    /// <summary>
    /// Notify all participants in a room that a new user has joined
    /// </summary>
    Task NotifyUserJoinedAsync(
        Guid roomId,
        Guid userId,
        string userName,
        string? avatarUrl,
        int filledSlots,
        int totalSlots,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants in a room that a user has left
    /// </summary>
    Task NotifyUserLeftAsync(
        Guid roomId,
        Guid userId,
        string userName,
        int filledSlots,
        int totalSlots,
        bool isRoomDeleted,
        Guid? newHostId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the room status has changed
    /// </summary>
    Task NotifyRoomStatusChangedAsync(
        Guid roomId,
        string newStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that a player updated their team assignment or position
    /// </summary>
    Task NotifyParticipantUpdatedAsync(
        Guid roomId,
        Guid userId,
        string userName,
        string? avatarUrl,
        string teamAssignment,
        string? position,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify a specific user that they have been kicked from the room
    /// </summary>
    Task NotifyUserKickedAsync(
        Guid roomId,
        Guid kickedUserId,
        string kickedUserName,
        int filledSlots,
        int totalSlots,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a user's connection to a room group
    /// </summary>
    Task AddToRoomGroupAsync(string connectionId, Guid roomId);

    /// <summary>
    /// Remove a user's connection from a room group
    /// </summary>
    Task RemoveFromRoomGroupAsync(string connectionId, Guid roomId);

    /// <summary>
    /// Notify all participants in a room that a user has paid their deposit
    /// </summary>
    Task NotifyParticipantPaidAsync(
        Guid roomId,
        Guid userId,
        string userName,
        decimal amountPaid,
        decimal totalDepositCollected,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the booking has been created and confirmed
    /// </summary>
    Task NotifyBookingCreatedAsync(
        Guid roomId,
        Guid bookingId,
        DateTime matchDate,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the room privacy settings have been updated
    /// </summary>
    Task NotifyRoomPrivacyUpdatedAsync(
        Guid roomId,
        string visibility,
        bool isPrivate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that a team's formation has been updated
    /// Sends the same response structure as the UpdateFormation API
    /// </summary>
    Task NotifyFormationUpdatedAsync(
        object formationResponse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that a team's name has been updated
    /// </summary>
    Task NotifyTeamNameUpdatedAsync(
        Guid roomId,
        string team,
        string? teamName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that a player has checked in
    /// </summary>
    Task NotifyPlayerCheckedInAsync(
        Guid roomId,
        Guid userId,
        int checkedInCount,
        int totalParticipants,
        bool allCheckedIn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the match has started
    /// </summary>
    Task NotifyMatchStartedAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the match has ended and reviewing phase started
    /// </summary>
    Task NotifyMatchEndedAsync(
        Guid roomId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants about vote progress
    /// </summary>
    Task NotifyVoteProgressAsync(
        Guid roomId,
        int voteCount,
        int totalParticipants,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all participants that the match result has been finalized
    /// </summary>
    Task NotifyMatchResultFinalizedAsync(
        Guid roomId,
        string finalResult,
        int voteCount,
        CancellationToken cancellationToken = default);
}
