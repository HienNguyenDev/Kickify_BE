using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Events;

public class RoomInvitationSentEventHandler : INotificationHandler<RoomInvitationSentDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoomInvitationSentEventHandler> _logger;

    public RoomInvitationSentEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<RoomInvitationSentEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RoomInvitationSentDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RoomInvitationSentEventHandler triggered. InvitationId: {InvitationId}, RoomId: {RoomId}, InviterId: {InviterId}, InviteeId: {InviteeId}",
            notification.InvitationId,
            notification.RoomId,
            notification.InviterId,
            notification.InviteeId);

        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Lời mời vào phòng";
        var body = $"{notification.InviterName} đã mời bạn tham gia phòng \"{notification.RoomName ?? "trận đấu"}\"";

        // Always create notification entity for history tracking
        var notificationEntity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.InviteeId,
            SenderId = notification.InviterId,
            NotificationType = NotificationType.MatchRoom,
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
        var preference = await _notificationPreferenceRepository.GetByUserIdAsync(notification.InviteeId, cancellationToken);

        if (preference is { MatchRoom: false })
        {
            _logger.LogInformation(
                "User {InviteeId} has disabled MatchRoom notifications. Skipping push notification.",
                notification.InviteeId);
            return;
        }

        // Send push notification
        var data = new Dictionary<string, string>
        {
            { "type", "room_invitation" },
            { "roomId", notification.RoomId.ToString() },
            { "invitationId", notification.InvitationId.ToString() },
            { "inviterId", notification.InviterId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(
                notification.InviteeId,
                title,
                body,
                data,
                cancellationToken);

            _logger.LogInformation("Push notification sent successfully to user {InviteeId}", notification.InviteeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {InviteeId}", notification.InviteeId);
        }
    }
}
