using Kickify.Api.Hubs;
using Kickify.Application.Abstractions.Services;
using Kickify.Infrastructure.ChatConnection;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Kickify.Api.Services;

public class MatchRoomHubService : IMatchRoomHubService
{
    private readonly IHubContext<MatchRoomHub> _hubContext;
    private readonly ConnectionMapping _connectionMapping;
    private readonly ILogger<MatchRoomHubService> _logger;

    public MatchRoomHubService(
        IHubContext<MatchRoomHub> hubContext,
        ConnectionMapping connectionMapping,
        ILogger<MatchRoomHubService> logger)
    {
        _hubContext = hubContext;
        _connectionMapping = connectionMapping;
        _logger = logger;
    }

    private void LogOutboundEvent(string eventName, string target, object payload)
    {
        _logger.LogInformation(
            "\ud83d\ude80 [SignalR Outbound] Event: {EventName} | Target: {Target} | Payload: {Payload}",
            eventName,
            target,
            JsonSerializer.Serialize(payload));
    }

    public async Task NotifyUserJoinedAsync(
        Guid roomId,
        Guid userId,
        string userName,
        string? avatarUrl,
        int filledSlots,
        int totalSlots,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            UserId = userId,
            UserName = userName,
            AvatarUrl = avatarUrl,
            FilledSlots = filledSlots,
            TotalSlots = totalSlots,
            JoinedAt = DateTime.UtcNow
        };

        LogOutboundEvent("UserJoinedRoom", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("UserJoinedRoom", payload, cancellationToken);
    }

    public async Task NotifyUserLeftAsync(
        Guid roomId,
        Guid userId,
        string userName,
        int filledSlots,
        int totalSlots,
        bool isRoomDeleted,
        Guid? newHostId,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            UserId = userId,
            UserName = userName,
            FilledSlots = filledSlots,
            TotalSlots = totalSlots,
            IsRoomDeleted = isRoomDeleted,
            NewHostId = newHostId,
            LeftAt = DateTime.UtcNow
        };

        LogOutboundEvent("UserLeftRoom", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("UserLeftRoom", payload, cancellationToken);
    }

    public async Task NotifyRoomStatusChangedAsync(
        Guid roomId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            Status = newStatus,
            ChangedAt = DateTime.UtcNow
        };

        LogOutboundEvent("RoomStatusChanged", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("RoomStatusChanged", payload, cancellationToken);
    }

    public async Task NotifyParticipantUpdatedAsync(
        Guid roomId,
        Guid userId,
        string userName,
        string? avatarUrl,
        string teamAssignment,
        string? position,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            UserId = userId,
            UserName = userName,
            AvatarUrl = avatarUrl,
            TeamAssignment = teamAssignment,
            Position = position,
            UpdatedAt = DateTime.UtcNow
        };

        LogOutboundEvent("ParticipantUpdated", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("ParticipantUpdated", payload, cancellationToken);
    }

    public async Task NotifyUserKickedAsync(
        Guid roomId,
        Guid kickedUserId,
        string kickedUserName,
        int filledSlots,
        int totalSlots,
        CancellationToken cancellationToken = default)
    {
        // 1. Notify the kicked user specifically
        var kickedPayload = new
        {
            RoomId = roomId,
            KickedAt = DateTime.UtcNow
        };

        var kickedUserConnections = _connectionMapping.GetConnections(kickedUserId);
        foreach (var connectionId in kickedUserConnections)
        {
            LogOutboundEvent("YouWereKicked", $"Client: {connectionId}", kickedPayload);
            await _hubContext.Clients.Client(connectionId).SendAsync("YouWereKicked", kickedPayload, cancellationToken);
        }

        // 2. Notify all remaining participants in the room
        var groupPayload = new
        {
            RoomId = roomId,
            KickedUserId = kickedUserId,
            KickedUserName = kickedUserName,
            FilledSlots = filledSlots,
            TotalSlots = totalSlots,
            KickedAt = DateTime.UtcNow
        };

        LogOutboundEvent("UserKicked", $"Group: room_{roomId}", groupPayload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("UserKicked", groupPayload, cancellationToken);
    }

    public async Task AddToRoomGroupAsync(string connectionId, Guid roomId)
    {
        _logger.LogInformation(
            "\ud83d\udd17 [SignalR Group] Action: AddToGroup | ConnectionId: {ConnectionId} | Group: room_{RoomId}",
            connectionId, roomId);
        await _hubContext.Groups.AddToGroupAsync(connectionId, $"room_{roomId}");
    }

    public async Task RemoveFromRoomGroupAsync(string connectionId, Guid roomId)
    {
        _logger.LogInformation(
            "\ud83d\udd17 [SignalR Group] Action: RemoveFromGroup | ConnectionId: {ConnectionId} | Group: room_{RoomId}",
            connectionId, roomId);
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"room_{roomId}");
    }

    public async Task NotifyParticipantPaidAsync(
        Guid roomId,
        Guid userId,
        string userName,
        decimal amountPaid,
        decimal totalDepositCollected,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            UserId = userId,
            UserName = userName,
            AmountPaid = amountPaid,
            TotalDepositCollected = totalDepositCollected,
            PaidAt = DateTime.UtcNow
        };

        LogOutboundEvent("ParticipantPaid", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("ParticipantPaid", payload, cancellationToken);
    }

    public async Task NotifyBookingCreatedAsync(
        Guid roomId,
        Guid bookingId,
        DateTime matchDate,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            BookingId = bookingId,
            MatchDate = matchDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = "Confirmed",
            ConfirmedAt = DateTime.UtcNow
        };

        LogOutboundEvent("BookingConfirmed", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("BookingConfirmed", payload, cancellationToken);
    }

    public async Task NotifyRoomPrivacyUpdatedAsync(
        Guid roomId,
        string visibility,
        bool isPrivate,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            Visibility = visibility,
            IsPrivate = isPrivate,
            UpdatedAt = DateTime.UtcNow
        };

        LogOutboundEvent("RoomPrivacyUpdated", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("RoomPrivacyUpdated", payload, cancellationToken);
    }

    public async Task NotifyFormationUpdatedAsync(
        object formationResponse,
        CancellationToken cancellationToken = default)
    {
        // formationResponse is UpdateFormationResponse - send it directly
        // to ensure SignalR payload matches API response exactly
        dynamic response = formationResponse;

        LogOutboundEvent("FormationUpdated", $"Group: room_{response.RoomId}", formationResponse);

        await _hubContext.Clients
            .Group($"room_{response.RoomId}")
            .SendAsync("FormationUpdated", formationResponse, cancellationToken);
    }

    public async Task NotifyTeamNameUpdatedAsync(
        Guid roomId,
        string team,
        string? teamName,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            Team = team,
            TeamName = teamName,
            UpdatedAt = DateTime.UtcNow
        };

        LogOutboundEvent("TeamNameUpdated", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("TeamNameUpdated", payload, cancellationToken);
    }

    public async Task NotifyPlayerCheckedInAsync(
        Guid roomId,
        Guid userId,
        int checkedInCount,
        int totalParticipants,
        bool allCheckedIn,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            UserId = userId,
            CheckedInCount = checkedInCount,
            TotalParticipants = totalParticipants,
            AllCheckedIn = allCheckedIn,
            CheckedInAt = DateTime.UtcNow
        };

        LogOutboundEvent("PlayerCheckedIn", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("PlayerCheckedIn", payload, cancellationToken);
    }

    public async Task NotifyMatchStartedAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            StartedAt = DateTime.UtcNow
        };

        LogOutboundEvent("MatchStarted", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("MatchStarted", payload, cancellationToken);
    }

    public async Task NotifyMatchEndedAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            EndedAt = DateTime.UtcNow,
            Message = "Tr?n ??u k?t th�c. Vui l�ng vote k?t qu? tr?n ??u trong v�ng 12 gi?."
        };

        LogOutboundEvent("MatchEnded", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("MatchEnded", payload, cancellationToken);
    }

    public async Task NotifyVoteProgressAsync(
        Guid roomId,
        int voteCount,
        int totalParticipants,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            VoteCount = voteCount,
            TotalParticipants = totalParticipants,
            Percentage = (double)voteCount / totalParticipants,
            UpdatedAt = DateTime.UtcNow
        };

        LogOutboundEvent("VoteProgress", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("VoteProgress", payload, cancellationToken);
    }

    public async Task NotifyMatchResultFinalizedAsync(
        Guid roomId,
        string finalResult,
        int voteCount,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            RoomId = roomId,
            FinalResult = finalResult,
            VoteCount = voteCount,
            FinalizedAt = DateTime.UtcNow
        };

        LogOutboundEvent("MatchResultFinalized", $"Group: room_{roomId}", payload);

        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("MatchResultFinalized", payload, cancellationToken);
    }
}
