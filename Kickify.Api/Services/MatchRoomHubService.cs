using Kickify.Api.Hubs;
using Kickify.Application.Abstractions.Services;
using Kickify.Infrastructure.ChatConnection;
using Microsoft.AspNetCore.SignalR;

namespace Kickify.Api.Services;

public class MatchRoomHubService : IMatchRoomHubService
{
    private readonly IHubContext<MatchRoomHub> _hubContext;
    private readonly ConnectionMapping _connectionMapping;

    public MatchRoomHubService(
        IHubContext<MatchRoomHub> hubContext,
        ConnectionMapping connectionMapping)
    {
        _hubContext = hubContext;
        _connectionMapping = connectionMapping;
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
        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("UserJoinedRoom", new
            {
                RoomId = roomId,
                UserId = userId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                FilledSlots = filledSlots,
                TotalSlots = totalSlots,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);
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
        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("UserLeftRoom", new
            {
                RoomId = roomId,
                UserId = userId,
                UserName = userName,
                FilledSlots = filledSlots,
                TotalSlots = totalSlots,
                IsRoomDeleted = isRoomDeleted,
                NewHostId = newHostId,
                LeftAt = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task NotifyRoomStatusChangedAsync(
        Guid roomId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("RoomStatusChanged", new
            {
                RoomId = roomId,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow
            }, cancellationToken);
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
        await _hubContext.Clients
            .Group($"room_{roomId}")
            .SendAsync("ParticipantUpdated", new
            {
                RoomId = roomId,
                UserId = userId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                TeamAssignment = teamAssignment,
                Position = position,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task AddToRoomGroupAsync(string connectionId, Guid roomId)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, $"room_{roomId}");
    }

    public async Task RemoveFromRoomGroupAsync(string connectionId, Guid roomId)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"room_{roomId}");
    }
}
