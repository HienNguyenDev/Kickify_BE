using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Posts.Commands.LikePost;

public class PostLikedEventHandler : INotificationHandler<PostLikedDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostLikedEventHandler> _logger;

    public PostLikedEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PostLikedEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PostLikedDomainEvent notification, CancellationToken cancellationToken)
    {
        var deepLink = $"kickify://post/{notification.PostId}";
        var title = "Lượt thích bài viết";
        var body = $"{notification.ActorName} đã thích bài viết của bạn";

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
            { "type", "post_liked" },
            { "postId", notification.PostId.ToString() },
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
            _logger.LogError(ex, "Failed to send post liked push to user {RecipientId}", notification.RecipientUserId);
        }
    }
}
