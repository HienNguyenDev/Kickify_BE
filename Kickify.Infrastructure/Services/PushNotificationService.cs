using FirebaseAdmin.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(IUserRepository userRepository, ILogger<PushNotificationService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.FcmToken == null)
        {
            _logger.LogWarning("Cannot send push notification to user {UserId}: FcmToken is null", userId);
            return;
        }

        _logger.LogInformation("Sending push notification to user {UserId} with token {FcmToken}", userId, user.FcmToken[..20] + "...");
        await SendToTokenAsync(user.FcmToken, title, body, data, cancellationToken);
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
        var tokens = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken))
            .Select(u => u.FcmToken!)
            .ToList();

        if (tokens.Count == 0)
        {
            _logger.LogWarning("No valid FCM tokens found for users");
            return;
        }

        await SendToTokensAsync(tokens, title, body, data, cancellationToken);
    }

    public async Task SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1,
                        ContentAvailable = true
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("Push notification sent successfully. Message ID: {MessageId}", response);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase push notification failed. ErrorCode: {ErrorCode}, MessagingErrorCode: {MessagingErrorCode}", 
                ex.ErrorCode, ex.MessagingErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification");
        }
    }

    public async Task SendToTokensAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var tokenList = fcmTokens.ToList();
        if (tokenList.Count == 0) return;

        try
        {
            var message = new MulticastMessage
            {
                Tokens = tokenList,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1,
                        ContentAvailable = true
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
            _logger.LogInformation("Multicast push notification sent. Success: {SuccessCount}, Failure: {FailureCount}", 
                response.SuccessCount, response.FailureCount);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase multicast push notification failed. ErrorCode: {ErrorCode}", ex.ErrorCode);
        }
    }

    public async Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1,
                        ContentAvailable = true
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
            _logger.LogInformation("Topic push notification sent successfully. Topic: {Topic}, MessageId: {MessageId}", topic, response);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase topic push notification failed. Topic: {Topic}, ErrorCode: {ErrorCode}", topic, ex.ErrorCode);
        }
    }
}
