using Kickify.Application.Abstractions.Repositories;
using Kickify.Infrastructure.ChatConnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kickify.Api.Hubs;

[Authorize]
public class MatchRoomHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ConnectionMapping _connectionMapping;

    public MatchRoomHub(
        IMediator mediator,
        ConnectionMapping connectionMapping)
    {
        _mediator = mediator;
        _connectionMapping = connectionMapping;
    }

    private Guid CurrentUserId => Guid.Parse(
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new HubException("Unauthorized"));

    private string CurrentUserName =>
        Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        _connectionMapping.Add(userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = CurrentUserId;
            _connectionMapping.Remove(userId, Context.ConnectionId);
        }
        catch
        {
            // Ignore errors during disconnect
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a SignalR group for a specific room to receive real-time updates
    /// </summary>
    public async Task JoinRoomGroup(Guid roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
    }

    /// <summary>
    /// Leave a SignalR group for a specific room
    /// </summary>
    public async Task LeaveRoomGroup(Guid roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room_{roomId}");
    }
}
