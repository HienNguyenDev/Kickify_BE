using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Comments.Commands.CreateComment;

public class PostCommentCreatedEventHandler : INotificationHandler<PostCommentCreatedDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostCommentCreatedEventHandler> _logger;

    public PostCommentCreatedEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PostCommentCreatedEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PostCommentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var deepLink = $"kickify://post/{notification.PostId}";
        var title = notification.IsReply ? "Phản hồi bình luận" : "Bình luận mới";
        var body = notification.IsReply
            ? $"{notification.ActorName} đã trả lời bình luận của bạn"
            : $"{notification.ActorName} đã bình luận bài viết của bạn";

        var notificationEntity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.RecipientUserId,
            SenderId = notification.ActorId,
            NotificationType = NotificationType.Post,
            Title = title,
            Message = body,
            DeepLink = deepLink,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notificationEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var preference = await _notificationPreferenceRepository
            .GetByUserIdAsync(notification.RecipientUserId, cancellationToken);

        if (preference is { Post: false })
        {
            _logger.LogInformation(
                "User {RecipientId} disabled Post notifications. Skipping push.",
                notification.RecipientUserId);
            return;
        }

        var data = new Dictionary<string, string>
        {
            { "type", notification.IsReply ? "post_comment_reply" : "post_commented" },
            { "postId", notification.PostId.ToString() },
            { "commentId", notification.CommentId.ToString() },
            { "actorId", notification.ActorId.ToString() },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(
                notification.RecipientUserId,
                title,
                body,
                data,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send comment push to user {RecipientId}", notification.RecipientUserId);
        }
    }
}
