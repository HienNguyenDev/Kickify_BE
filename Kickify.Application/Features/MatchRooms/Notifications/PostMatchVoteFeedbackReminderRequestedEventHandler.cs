using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Notifications;

public class PostMatchVoteFeedbackReminderRequestedEventHandler
    : INotificationHandler<PostMatchVoteFeedbackReminderRequestedDomainEvent>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IMatchResultVoteRepository _matchResultVoteRepository;
    private readonly IMatchFeedbackRepository _matchFeedbackRepository;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostMatchVoteFeedbackReminderRequestedEventHandler> _logger;

    public PostMatchVoteFeedbackReminderRequestedEventHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IMatchResultVoteRepository matchResultVoteRepository,
        IMatchFeedbackRepository matchFeedbackRepository,
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PostMatchVoteFeedbackReminderRequestedEventHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _matchResultVoteRepository = matchResultVoteRepository;
        _matchFeedbackRepository = matchFeedbackRepository;
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PostMatchVoteFeedbackReminderRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Attempt is not (1 or 2))
        {
            _logger.LogWarning("Unsupported post-match reminder attempt {A} for room {RoomId}", notification.Attempt, notification.RoomId);
            return;
        }

        var room = await _matchRoomRepository.GetByIdAsync(notification.RoomId);
        if (room is null || room.Status != RoomStatus.Reviewing)
            return;

        var participants = await _roomParticipantRepository.GetParticipantsByRoomAsync(notification.RoomId, cancellationToken);
        if (participants.Count == 0)
            return;

        var feedbacks = await _matchFeedbackRepository.GetFeedbacksByMatchAsync(notification.RoomId, cancellationToken);
        var reviewers = feedbacks.Select(f => f.ReviewerId).ToHashSet();

        var roomLabel = string.IsNullOrWhiteSpace(room.RoomName) ? "Trận vừa xong" : $"\"{room.RoomName}\"";
        var deepLink = $"kickify://room/{notification.RoomId}";
        var title = "Nhắc vote kết quả & feedback";
        var bodyAttempt = notification.Attempt == 1
            ? $"{roomLabel} đã kết thúc. Vào phòng trận để bình chọn kết quả và gửi đánh giá đồng đội."
            : $"{roomLabel}: bạn vẫn chưa hoàn tất bình chọn kết quả hoặc feedback. Hãy hoàn thành trong app để khóa sổ trận.";

        foreach (var p in participants.DistinctBy(x => x.UserId))
        {
            var voted = await _matchResultVoteRepository.HasUserVotedAsync(notification.RoomId, p.UserId, cancellationToken);
            var gaveFeedback = reviewers.Contains(p.UserId);
            if (voted && gaveFeedback)
                continue;

            await _notificationRepository.AddAsync(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = p.UserId,
                SenderId = null,
                NotificationType = NotificationType.MatchRoom,
                Title = title,
                Message = bodyAttempt,
                DeepLink = deepLink,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var data = new Dictionary<string, string>
        {
            { "type", "match_room_vote_feedback_reminder" },
            { "roomId", notification.RoomId.ToString() },
            { "attempt", notification.Attempt.ToString() },
            { "deepLink", deepLink }
        };

        foreach (var p in participants.DistinctBy(x => x.UserId))
        {
            var voted = await _matchResultVoteRepository.HasUserVotedAsync(notification.RoomId, p.UserId, cancellationToken);
            var gaveFeedback = reviewers.Contains(p.UserId);
            if (voted && gaveFeedback)
                continue;

            var preference = await _notificationPreferenceRepository.GetByUserIdAsync(p.UserId, cancellationToken);
            if (preference is { MatchRoom: false })
                continue;

            try
            {
                await _pushNotificationService.SendToUserAsync(p.UserId, title, bodyAttempt, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-match reminder push failed user {UserId} room {RoomId}", p.UserId, notification.RoomId);
            }
        }
    }
}
