using Kickify.Api.Hubs;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Kickify.Infrastructure.ChatConnection;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Kickify.Api.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ConnectionMapping _connectionMapping;
    private readonly ILogger<ChatHubService> _logger;

    public ChatHubService(
        IHubContext<ChatHub> hubContext,
        ConnectionMapping connectionMapping,
        ILogger<ChatHubService> logger)
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

    public async Task SendMessageToUserAsync(
        Guid userId,
        ChatMessageDto message,
        CancellationToken cancellationToken = default)
    {
        var connectionIds = _connectionMapping.GetConnections(userId).ToList();

        if (connectionIds.Any())
        {
            LogOutboundEvent("ReceiveMessage", $"Clients: [{string.Join(", ", connectionIds)}]", message);

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
            var payload = new
            {
                UserId = fromUserId,
                UserName = fromUserName
            };

            LogOutboundEvent("UserTyping", $"Clients: [{string.Join(", ", connectionIds)}]", payload);

            await _hubContext.Clients
                .Clients(connectionIds)
                .SendAsync("UserTyping", payload, cancellationToken);
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
            var payload = new { ByUserId = byUserId };

            LogOutboundEvent("MessagesRead", $"Clients: [{string.Join(", ", connectionIds)}]", payload);

            await _hubContext.Clients
                .Clients(connectionIds)
                .SendAsync("MessagesRead", payload, cancellationToken);
        }
    }

    public async Task NotifyOnlineStatusAsync(
        Guid userId,
        bool isOnline,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            UserId = userId,
            IsOnline = isOnline
        };

        LogOutboundEvent("UserOnlineStatus", "All Clients", payload);

        await _hubContext.Clients.All
            .SendAsync("UserOnlineStatus", payload, cancellationToken);
    }
}
