using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class FriendRequestSentEventHandler : INotificationHandler<FriendRequestSentDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;

    public FriendRequestSentEventHandler(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    public async Task Handle(FriendRequestSentDomainEvent notification, CancellationToken cancellationToken)
    {
        var title = "L?i m?i k?t b?n m?i";
        var body = $"{notification.RequesterName} ?ã g?i cho b?n m?t l?i m?i k?t b?n";

        var data = new Dictionary<string, string>
        {
            { "type", "friend_request" },
            { "friendshipId", notification.FriendshipId.ToString() },
            { "requesterId", notification.RequesterId.ToString() }
        };

        await _pushNotificationService.SendToUserAsync(
            notification.AddresseeId,
            title,
            body,
            data,
            cancellationToken);
    }
}
