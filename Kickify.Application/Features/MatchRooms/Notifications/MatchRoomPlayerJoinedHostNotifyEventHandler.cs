using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class MatchRoomPlayerJoinedHostNotifyEventHandler : INotificationHandler<MatchRoomPlayerJoinedHostNotifyDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MatchRoomPlayerJoinedHostNotifyEventHandler> _logger;

    public MatchRoomPlayerJoinedHostNotifyEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MatchRoomPlayerJoinedHostNotifyEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MatchRoomPlayerJoinedHostNotifyDomainEvent notification, CancellationToken cancellationToken)
    {
        var roomLabel = string.IsNullOrWhiteSpace(notification.RoomName) ? "phòng trận" : $"\"{notification.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Có người tham gia phòng";
        var body = $"{notification.JoinerDisplayName} đã tham gia {roomLabel} của bạn.";

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
        {
            _logger.LogInformation("Host {HostId} disabled MatchRoom push. Skipping join notification.", notification.HostUserId);
            return;
        }

        var data = new Dictionary<string, string>
        {
            { "type", "match_room_player_joined" },
            { "roomId", notification.RoomId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(notification.HostUserId, title, body, data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push failed for host {HostId} join notification", notification.HostUserId);
        }
    }
}
