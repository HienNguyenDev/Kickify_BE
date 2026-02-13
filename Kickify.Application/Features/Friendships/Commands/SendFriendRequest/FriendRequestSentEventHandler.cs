using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class FriendRequestSentEventHandler : INotificationHandler<FriendRequestSentDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<FriendRequestSentEventHandler> _logger;

    public FriendRequestSentEventHandler(
        IPushNotificationService pushNotificationService,
        ILogger<FriendRequestSentEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(FriendRequestSentDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "FriendRequestSentEventHandler triggered. FriendshipId: {FriendshipId}, RequesterId: {RequesterId}, AddresseeId: {AddresseeId}, RequesterName: {RequesterName}",
            notification.FriendshipId,
            notification.RequesterId,
            notification.AddresseeId,
            notification.RequesterName);

        var title = "L?i m?i k?t b?n m?i";
        var body = $"{notification.RequesterName} ?ã g?i cho b?n m?t l?i m?i k?t b?n";

        var data = new Dictionary<string, string>
        {
            { "type", "friend_request" },
            { "friendshipId", notification.FriendshipId.ToString() },
            { "requesterId", notification.RequesterId.ToString() }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(
                notification.AddresseeId,
                title,
                body,
                data,
                cancellationToken);

            _logger.LogInformation("Push notification sent successfully to user {AddresseeId}", notification.AddresseeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {AddresseeId}", notification.AddresseeId);
        }
    }
}
