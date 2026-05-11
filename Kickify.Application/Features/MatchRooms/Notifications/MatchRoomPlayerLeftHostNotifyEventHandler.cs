using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class MatchRoomPlayerLeftHostNotifyEventHandler : INotificationHandler<MatchRoomPlayerLeftHostNotifyDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MatchRoomPlayerLeftHostNotifyEventHandler> _logger;

    public MatchRoomPlayerLeftHostNotifyEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MatchRoomPlayerLeftHostNotifyEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MatchRoomPlayerLeftHostNotifyDomainEvent notification, CancellationToken cancellationToken)
    {
        var roomLabel = string.IsNullOrWhiteSpace(notification.RoomName) ? "phòng trận" : $"\"{notification.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Thành viên rời phòng";
        var body = $"{notification.LeaverDisplayName} đã rời {roomLabel}. Bạn có thể mời người chơi khác để đủ đội hình.";

        var entity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.HostUserId,
            SenderId = null,
            NotificationType = NotificationType.MatchRoom,
            Title = title,
            Message = body,
            DeepLink = deepLink,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var preference = await _notificationPreferenceRepository.GetByUserIdAsync(notification.HostUserId, cancellationToken);
        if (preference is { MatchRoom: false })
            return;

        var data = new Dictionary<string, string>
        {
            { "type", "match_room_player_left" },
            { "roomId", notification.RoomId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(notification.HostUserId, title, body, data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push failed for host leave notification {HostId}", notification.HostUserId);
        }
    }
}
