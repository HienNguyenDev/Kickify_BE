using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class PlayerKickedFromMatchRoomEventHandler : INotificationHandler<PlayerKickedFromMatchRoomDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlayerKickedFromMatchRoomEventHandler> _logger;

    public PlayerKickedFromMatchRoomEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PlayerKickedFromMatchRoomEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PlayerKickedFromMatchRoomDomainEvent notification, CancellationToken cancellationToken)
    {
        var roomLabel = string.IsNullOrWhiteSpace(notification.RoomName) ? "phòng trận" : $"\"{notification.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Bạn đã bị mời khỏi phòng";
        var reasonPart = string.IsNullOrWhiteSpace(notification.Reason)
            ? string.Empty
            : $" Lý do: {notification.Reason}";
        var body = $"Chủ phòng đã xóa bạn khỏi {roomLabel}.{reasonPart}";

        var entity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.KickedUserId,
            SenderId = null,
            NotificationType = NotificationType.MatchRoom,
            Title = title,
            Message = body.Trim(),
            DeepLink = deepLink,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var preference = await _notificationPreferenceRepository.GetByUserIdAsync(notification.KickedUserId, cancellationToken);
        if (preference is { MatchRoom: false })
            return;

        var data = new Dictionary<string, string>
        {
            { "type", "match_room_kicked" },
            { "roomId", notification.RoomId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(notification.KickedUserId, title, body.Trim(), data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push failed for kicked user {UserId}", notification.KickedUserId);
        }
    }
}
