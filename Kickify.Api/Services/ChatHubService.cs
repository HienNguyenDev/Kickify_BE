using Kickify.Api.Hubs;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Kickify.Infrastructure.ChatConnection;
using Microsoft.AspNetCore.SignalR;

namespace Kickify.Api.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ConnectionMapping _connectionMapping;

    public ChatHubService(
        IHubContext<ChatHub> hubContext,
        ConnectionMapping connectionMapping)
    {
        _hubContext = hubContext;
        _connectionMapping = connectionMapping;
    }

    public async Task SendMessageToUserAsync(
        Guid userId,
        ChatMessageDto message,
        CancellationToken cancellationToken = default)
    {
        var connectionIds = _connectionMapping.GetConnections(userId).ToList();

        if (connectionIds.Any())
        {
            await _hubContext.Clients
                .Clients(connectionIds)
                .SendAsync("ReceiveMessage", message, cancellationToken);
        }
    }

    public async Task NotifyTypingAsync(
        Guid toUserId,
        Guid fromUserId,
        string fromUserName,
        CancellationToken cancellationToken = default)
    {
        var connectionIds = _connectionMapping.GetConnections(toUserId).ToList();

        if (connectionIds.Any())
        {
            await _hubContext.Clients
                .Clients(connectionIds)
                .SendAsync("UserTyping", new
                {
                    UserId = fromUserId,
                    UserName = fromUserName
                }, cancellationToken);
        }
    }

    public async Task NotifyMessageReadAsync(
        Guid toUserId,
        Guid byUserId,
        CancellationToken cancellationToken = default)
    {
        var connectionIds = _connectionMapping.GetConnections(toUserId).ToList();

        if (connectionIds.Any())
        {
            await _hubContext.Clients
                .Clients(connectionIds)
                .SendAsync("MessagesRead", new { ByUserId = byUserId }, cancellationToken);
        }
    }

    public async Task NotifyOnlineStatusAsync(
        Guid userId,
        bool isOnline,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All
            .SendAsync("UserOnlineStatus", new
            {
                UserId = userId,
                IsOnline = isOnline
            }, cancellationToken);
    }
}
