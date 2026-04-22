namespace Kickify.Application.Abstractions.Services;

public interface IPushNotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToTokensAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SubscribeToTopicAsync(string fcmToken, string topic, CancellationToken cancellationToken = default);
    Task UnsubscribeFromTopicAsync(string fcmToken, string topic, CancellationToken cancellationToken = default);
}
