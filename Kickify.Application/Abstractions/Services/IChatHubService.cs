using Kickify.Application.DTOs;

namespace Kickify.Application.Abstractions.Services;

public interface IChatHubService
{
    Task SendMessageToUserAsync(Guid userId, DTOs.ChatMessageDto message, CancellationToken cancellationToken = default);
    Task NotifyTypingAsync(Guid toUserId, Guid fromUserId, string fromUserName, CancellationToken cancellationToken = default);
    Task NotifyMessageReadAsync(Guid toUserId, Guid byUserId, CancellationToken cancellationToken = default);
    Task NotifyOnlineStatusAsync(Guid userId, bool isOnline, CancellationToken cancellationToken = default);
}


