using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Chat.Commands.SendRoomMessage;
using Kickify.Application.Features.Chat.Queries.GetRoomMessages;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.ChatConnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;

namespace Kickify.Api.Hubs;

[Authorize]
public class MatchRoomHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ConnectionMapping _connectionMapping;
    private readonly ILogger<MatchRoomHub> _logger;

    public MatchRoomHub(
        IMediator mediator,
        ConnectionMapping connectionMapping,
        ILogger<MatchRoomHub> logger)
    {
        _mediator = mediator;
        _connectionMapping = connectionMapping;
        _logger = logger;
    }

    private Guid CurrentUserId => Guid.Parse(
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new HubException("Unauthorized"));

    private string CurrentUserName =>
        Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

    private void LogInboundEvent(string methodName, object? payload = null)
    {
        var payloadJson = payload != null ? JsonSerializer.Serialize(payload) : "{}";
        _logger.LogInformation(
            "\ud83d\udce5 [SignalR Inbound] Method: {MethodName} | Caller UserId: {UserId} | Payload: {Payload}",
            methodName,
            CurrentUserId,
            payloadJson);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        _connectionMapping.Add(userId, Context.ConnectionId);

        _logger.LogInformation(
            "\ud83d\udce5 [SignalR Connected] UserId: {UserId} | ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = CurrentUserId;
            _connectionMapping.Remove(userId, Context.ConnectionId);

            _logger.LogInformation(
                "\ud83d\udce5 [SignalR Disconnected] UserId: {UserId} | ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }
        catch
        {
            // Ignore errors during disconnect
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a SignalR group for a specific room to receive real-time updates.
    /// FIX: Split from original JoinRoomGroup(Guid roomId, string? teamAssignment = null)
    /// because SignalR matches methods by exact argument count — optional parameters
    /// are NOT supported when invoked from JS (sends 1-arg array, server finds no 1-arg match).
    /// </summary>
    public async Task JoinRoomGroup(Guid roomId)
    {
        LogInboundEvent(nameof(JoinRoomGroup), new { RoomId = roomId });
        await JoinRoomGroupInternalAsync(roomId, null);
    }

    /// <summary>
    /// Join a SignalR group for a specific room with explicit team assignment.
    /// Called with exactly 2 args from JS: connection.invoke("JoinRoomGroupWithTeam", roomId, teamAssignment)
    /// </summary>
    public async Task JoinRoomGroupWithTeam(Guid roomId, string teamAssignment)
    {
        LogInboundEvent(nameof(JoinRoomGroupWithTeam), new { RoomId = roomId, TeamAssignment = teamAssignment });
        await JoinRoomGroupInternalAsync(roomId, teamAssignment);
    }

    private async Task JoinRoomGroupInternalAsync(Guid roomId, string? teamAssignment)
    {
        // Join main room group (for room events)
        await Groups.AddToGroupAsync(Context.ConnectionId, GetRoomGroupName(roomId));

        // Join general chat channel
        await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.General));

        // Join team chat channel if assigned
        if (!string.IsNullOrEmpty(teamAssignment) && Enum.TryParse<TeamAssignment>(teamAssignment, out var team))
        {
            if (team == TeamAssignment.A)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamA));
            }
            else if (team == TeamAssignment.B)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamB));
            }
        }

        await Clients.Caller.SendAsync("JoinedRoom", new
        {
            RoomId = roomId,
            TeamAssignment = teamAssignment,
            UserId = CurrentUserId,
            UserName = CurrentUserName
        });
    }

    public async Task LeaveRoomGroup(Guid roomId)
    {
        LogInboundEvent(nameof(LeaveRoomGroup), new { RoomId = roomId });

        // Leave all room-related groups
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetRoomGroupName(roomId));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.General));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamA));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamB));

        await Clients.Caller.SendAsync("LeftRoom", new { RoomId = roomId });
    }

    public async Task SwitchTeamChat(Guid roomId, string newTeamAssignment)
    {
        LogInboundEvent(nameof(SwitchTeamChat), new { RoomId = roomId, NewTeamAssignment = newTeamAssignment });

        // Leave old team channels
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamA));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamB));

        // Join new team channel
        if (Enum.TryParse<TeamAssignment>(newTeamAssignment, out var team))
        {
            if (team == TeamAssignment.A)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamA));
            }
            else if (team == TeamAssignment.B)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamB));
            }
        }

        await Clients.Caller.SendAsync("TeamChatSwitched", new { RoomId = roomId, TeamAssignment = newTeamAssignment });
    }

    public async Task SendRoomMessage(Guid roomId, int channel, string messageText)
    {
        LogInboundEvent(nameof(SendRoomMessage), new { RoomId = roomId, Channel = channel, MessageText = messageText });

        try
        {
            var roomChannel = (RoomChatChannel)channel;

            var command = new SendRoomMessageCommand
            {
                SenderId = CurrentUserId,
                RoomId = roomId,
                Channel = roomChannel,
                MessageText = messageText
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var groupName = GetChatGroupName(roomId, roomChannel);
                await Clients.Group(groupName).SendAsync("ReceiveRoomMessage", result.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Error.Description });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u274c [SignalR Error] Method: {MethodName} | UserId: {UserId} | Error: {Error}",
                nameof(SendRoomMessage), CurrentUserId, ex.Message);
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task GetRoomMessages(Guid roomId, int channel, int page = 1, int pageSize = 50)
    {
        LogInboundEvent(nameof(GetRoomMessages), new { RoomId = roomId, Channel = channel, Page = page, PageSize = pageSize });

        try
        {
            var query = new GetRoomMessagesQuery
            {
                CurrentUserId = CurrentUserId,
                RoomId = roomId,
                Channel = (RoomChatChannel)channel,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                await Clients.Caller.SendAsync("ReceiveRoomMessages", result.Value);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Error.Description });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\u274c [SignalR Error] Method: {MethodName} | UserId: {UserId} | Error: {Error}",
                nameof(GetRoomMessages), CurrentUserId, ex.Message);
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task RoomTyping(Guid roomId, int channel)
    {
        LogInboundEvent(nameof(RoomTyping), new { RoomId = roomId, Channel = channel });

        var groupName = GetChatGroupName(roomId, (RoomChatChannel)channel);
        await Clients.OthersInGroup(groupName).SendAsync("RoomUserTyping", new
        {
            RoomId = roomId,
            Channel = channel,
            UserId = CurrentUserId,
            UserName = CurrentUserName
        });
    }

    public async Task RoomStopTyping(Guid roomId, int channel)
    {
        LogInboundEvent(nameof(RoomStopTyping), new { RoomId = roomId, Channel = channel });

        var groupName = GetChatGroupName(roomId, (RoomChatChannel)channel);
        await Clients.OthersInGroup(groupName).SendAsync("RoomUserStopTyping", new
        {
            RoomId = roomId,
            Channel = channel,
            UserId = CurrentUserId
        });
    }
    private static string GetRoomGroupName(Guid roomId) => $"room_{roomId}";

    private static string GetChatGroupName(Guid roomId, RoomChatChannel channel) => $"room_{roomId}_chat_{channel}";
}
