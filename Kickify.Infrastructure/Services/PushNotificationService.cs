using FirebaseAdmin.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IUserRepository _userRepository;

    public PushNotificationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.FcmToken == null) return;

        await SendToTokenAsync(user.FcmToken, title, body, data, cancellationToken);
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string body, Dictionary<string, string>? data = null, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByIdsAsync(userIds, cancellationToken);
        var tokens = users
            .Where(u => !string.IsNullOrEmpty(u.FcmToken))
            .Select(u => u.FcmToken!)
            .ToList();

        if (tokens.Count == 0) return;

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

            await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        }
        catch (FirebaseMessagingException)
        {
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

            await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
        }
        catch (FirebaseMessagingException)
        {
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

            await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        }
        catch (FirebaseMessagingException)
        {
        }
    }
}
