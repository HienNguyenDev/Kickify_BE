using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.Chat.Commands.SendRoomMessage;
using Kickify.Application.Features.Chat.Queries.GetRoomMessages;
using Kickify.Domain.Enums;
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
    public async Task JoinRoomGroup(Guid roomId, string? teamAssignment = null)
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
        // Leave all room-related groups
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetRoomGroupName(roomId));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.General));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamA));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(roomId, RoomChatChannel.TeamB));

        await Clients.Caller.SendAsync("LeftRoom", new { RoomId = roomId });
    }

    public async Task SwitchTeamChat(Guid roomId, string newTeamAssignment)
    {
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
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task GetRoomMessages(Guid roomId, int channel, int page = 1, int pageSize = 50)
    {
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
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task RoomTyping(Guid roomId, int channel)
    {
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
