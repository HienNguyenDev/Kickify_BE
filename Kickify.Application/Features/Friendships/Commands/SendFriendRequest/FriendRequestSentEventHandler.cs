using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class FriendRequestSentEventHandler : INotificationHandler<FriendRequestSentDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FriendRequestSentEventHandler> _logger;

    public FriendRequestSentEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<FriendRequestSentEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
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

        var title = "Lời mời kết bạn mới";
        var body = $"{notification.RequesterName} đã gửi cho bạn một lời mời kết bạn mới";
        var deepLink = "kickify://friends/requests";

        // Always create notification entity for history tracking
        var notificationEntity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.AddresseeId,
            SenderId = notification.RequesterId,
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
            "Notification entity created. NotificationId: {NotificationId}, UserId: {UserId}",
            notificationEntity.NotificationId,
            notificationEntity.UserId);

        // Check notification preference before sending push notification
        var preference = await _notificationPreferenceRepository
            .GetByUserIdAsync(notification.AddresseeId, cancellationToken);

        if (preference is { Friendship: false })
        {
            _logger.LogInformation(
                "User {AddresseeId} has disabled Friendship notifications. Skipping push notification.",
                notification.AddresseeId);
            return;
        }

        // Send push notification
        var data = new Dictionary<string, string>
        {
            { "type", "friend_request" },
            { "friendshipId", notification.FriendshipId.ToString() },
            { "requesterId", notification.RequesterId.ToString() },
            { "deepLink", deepLink }
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

