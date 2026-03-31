using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class FriendRequestAcceptedEventHandler : INotificationHandler<FriendRequestAcceptedDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FriendRequestAcceptedEventHandler> _logger;

    public FriendRequestAcceptedEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<FriendRequestAcceptedEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(FriendRequestAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "FriendRequestAcceptedEventHandler triggered. FriendshipId: {FriendshipId}, RequesterId: {RequesterId}, AddresseeId: {AddresseeId}, AddresseeName: {AddresseeName}",
            notification.FriendshipId,
            notification.RequesterId,
            notification.AddresseeId,
            notification.AddresseeName);

        var title = "Lời mời kết bạn đã được chấp nhận";
        var body = $"{notification.AddresseeName} đã chấp nhận lời mời kết bạn của bạn";
        var deepLink = "kickify://friends";

        var notificationEntity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.RequesterId,
            SenderId = notification.AddresseeId,
            NotificationType = NotificationType.Friendship,
            Title = title,
            Message = body,
            DeepLink = deepLink,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notificationEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification entity created for accepted friend request. NotificationId: {NotificationId}, UserId: {UserId}",
            notificationEntity.NotificationId,
            notificationEntity.UserId);

        var preference = await _notificationPreferenceRepository
            .GetByUserIdAsync(notification.RequesterId, cancellationToken);

        if (preference is { Friendship: false })
        {
            _logger.LogInformation(
                "User {RequesterId} has disabled Friendship notifications. Skipping push notification for accepted friend request.",
                notification.RequesterId);
            return;
        }

        var data = new Dictionary<string, string>
        {
            { "type", "friend_request_accepted" },
            { "friendshipId", notification.FriendshipId.ToString() },
            { "requesterId", notification.RequesterId.ToString() },
            { "addresseeId", notification.AddresseeId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(
                notification.RequesterId,
                title,
                body,
                data,
                cancellationToken);

            _logger.LogInformation("Push notification for accepted friend request sent successfully to user {RequesterId}", notification.RequesterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification for accepted friend request to user {RequesterId}", notification.RequesterId);
        }
    }
}

