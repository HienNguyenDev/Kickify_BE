using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class MatchRoomBecamePrivateNotifyParticipantsEventHandler
    : INotificationHandler<MatchRoomBecamePrivateNotifyParticipantsDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MatchRoomBecamePrivateNotifyParticipantsEventHandler> _logger;

    public MatchRoomBecamePrivateNotifyParticipantsEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MatchRoomBecamePrivateNotifyParticipantsEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MatchRoomBecamePrivateNotifyParticipantsDomainEvent notification, CancellationToken cancellationToken)
    {
        var roomLabel = string.IsNullOrWhiteSpace(notification.RoomName) ? "Phòng trận" : $"\"{notification.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Phòng chuyển sang riêng tư";
        var body = $"{roomLabel} hiện là phòng riêng (cần mật khẩu để vào). Hãy mở phòng trong app để xem chi tiết.";

        foreach (var userId in notification.ParticipantUserIds.Distinct())
        {
            await _notificationRepository.AddAsync(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                SenderId = null,
                NotificationType = NotificationType.MatchRoom,
                Title = title,
                Message = body,
                DeepLink = deepLink,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var data = new Dictionary<string, string>
        {
            { "type", "match_room_now_private" },
            { "roomId", notification.RoomId.ToString() },
            { "deepLink", deepLink }
        };

        foreach (var userId in notification.ParticipantUserIds.Distinct())
        {
            var preference = await _notificationPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);
            if (preference is { MatchRoom: false })
                continue;

            try
            {
                await _pushNotificationService.SendToUserAsync(userId, title, body, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push failed for private-room notify user {UserId}", userId);
            }
        }
    }
}
