using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class PreMatchReminderRequestedEventHandler : INotificationHandler<PreMatchReminderRequestedDomainEvent>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PreMatchReminderRequestedEventHandler> _logger;

    public PreMatchReminderRequestedEventHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PreMatchReminderRequestedEventHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PreMatchReminderRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.MinutesBefore is not (30 or 60))
        {
            _logger.LogWarning("Unsupported pre-match reminder window {M} for room {RoomId}", notification.MinutesBefore, notification.RoomId);
            return;
        }

        var room = await _matchRoomRepository.GetByIdAsync(notification.RoomId);
        if (room is null)
            return;

        if (room.Status is RoomStatus.Cancelled or RoomStatus.Completed)
            return;

        if (room.Status is RoomStatus.InProgress or RoomStatus.Reviewing)
            return;

        var participants = await _roomParticipantRepository.GetParticipantsByRoomAsync(notification.RoomId, cancellationToken);
        if (participants.Count == 0)
            return;

        var roomLabel = string.IsNullOrWhiteSpace(room.RoomName) ? "Trận đấu của bạn" : $"\"{room.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var m = notification.MinutesBefore;

        foreach (var p in participants.DistinctBy(x => x.UserId))
        {
            var checkedIn = p.CheckedIn;
            var title = checkedIn
                ? "Trận đấu sắp bắt đầu"
                : "Nhắc check-in & trận sắp diễn ra";
            var body = checkedIn
                ? $"{roomLabel} bắt đầu sau {m} phút. Hãy có mặt đúng giờ tại sân."
                : $"{roomLabel} bắt đầu sau {m} phút. Hãy check-in trong phòng trận trên app nếu bạn chưa làm.";

            await _notificationRepository.AddAsync(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = p.UserId,
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
            { "type", "match_room_pre_match_reminder" },
            { "roomId", notification.RoomId.ToString() },
            { "minutesBefore", m.ToString() },
            { "deepLink", deepLink }
        };

        foreach (var p in participants.DistinctBy(x => x.UserId))
        {
            var preference = await _notificationPreferenceRepository.GetByUserIdAsync(p.UserId, cancellationToken);
            if (preference is { MatchRoom: false })
                continue;

            var checkedIn = p.CheckedIn;
            var title = checkedIn
                ? "Trận đấu sắp bắt đầu"
                : "Nhắc check-in & trận sắp diễn ra";
            var body = checkedIn
                ? $"{roomLabel} bắt đầu sau {m} phút. Hãy có mặt đúng giờ tại sân."
                : $"{roomLabel} bắt đầu sau {m} phút. Hãy check-in trong phòng trận trên app nếu bạn chưa làm.";

            try
            {
                await _pushNotificationService.SendToUserAsync(p.UserId, title, body, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pre-match push failed for user {UserId} room {RoomId}", p.UserId, notification.RoomId);
            }
        }
    }
}
