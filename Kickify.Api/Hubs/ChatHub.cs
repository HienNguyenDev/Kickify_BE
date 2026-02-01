using Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;
using Kickify.Application.Features.Chat.Commands.SendPrivateMessage;
using Kickify.Application.Features.Chat.Queries.GetPrivateConversation;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.ChatConnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kickify.Api.Hubs;

/// <summary>
/// Hub for private 1-1 chat. For room chat, use MatchRoomHub.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ConnectionMapping _connectionMapping;

    public ChatHub(
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

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        _connectionMapping.Add(userId, Context.ConnectionId);

        await Clients.Others.SendAsync("UserOnlineStatus", new
        {
            UserId = userId,
            IsOnline = true
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = CurrentUserId;
            _connectionMapping.Remove(userId, Context.ConnectionId);

            if (!_connectionMapping.IsOnline(userId))
            {
                await Clients.Others.SendAsync("UserOnlineStatus", new
                {
                    UserId = userId,
                    IsOnline = false
                });
            }
        }
        catch { }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Private Chat

    public async Task SendPrivateMessage(Guid receiverId, string messageText, int messageType = 0)
    {
        try
        {
            var command = new SendPrivateMessageCommand
            {
                ReceiverId = receiverId,
                MessageText = messageText,
                MessageType = (MessageType)messageType
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Error.Description });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task Typing(Guid toUserId)
    {
        var connectionIds = _connectionMapping.GetConnections(toUserId).ToList();
        if (connectionIds.Any())
        {
            await Clients.Clients(connectionIds).SendAsync("UserTyping", new
            {
                UserId = CurrentUserId,
                UserName = CurrentUserName
            });
        }
    }

    public async Task StopTyping(Guid toUserId)
    {
        var connectionIds = _connectionMapping.GetConnections(toUserId).ToList();
        if (connectionIds.Any())
        {
            await Clients.Clients(connectionIds).SendAsync("UserStopTyping", new
            {
                UserId = CurrentUserId
            });
        }
    }

    public async Task MarkAsRead(Guid fromUserId)
    {
        try
        {
            var command = new MarkMessagesAsReadCommand { FromUserId = fromUserId };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("Error", new { Message = result.Error.Description });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", new { Message = ex.Message });
        }
    }

    public async Task GetConversation(Guid otherUserId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var query = new GetPrivateConversationQuery
            {
                OtherUserId = otherUserId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                await Clients.Caller.SendAsync("ReceiveConversation", result.Value);
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

    #endregion

    #region Online Status

    public async Task GetOnlineStatus(Guid userId)
    {
        var isOnline = _connectionMapping.IsOnline(userId);
        await Clients.Caller.SendAsync("UserOnlineStatus", new
        {
            UserId = userId,
            IsOnline = isOnline
        });
    }

    public async Task GetOnlineUsers()
    {
        var onlineUsers = _connectionMapping.GetOnlineUsers();
        await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
    }

    #endregion
}
