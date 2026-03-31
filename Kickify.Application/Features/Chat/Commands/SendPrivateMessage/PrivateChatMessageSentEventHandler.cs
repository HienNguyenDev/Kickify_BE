using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Chat.Commands.SendPrivateMessage;

public class PrivateChatMessageSentEventHandler : INotificationHandler<PrivateChatMessageSentDomainEvent>
{
    private const int PreviewMaxLength = 120;

    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PrivateChatMessageSentEventHandler> _logger;

    public PrivateChatMessageSentEventHandler(
        IPushNotificationService pushNotificationService,
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork,
        ILogger<PrivateChatMessageSentEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _notificationRepository = notificationRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PrivateChatMessageSentDomainEvent notification, CancellationToken cancellationToken)
    {
        var chatId = notification.SenderId.ToString();
        var deepLink = $"kickify://chat/{chatId}";
        var title = "Tin nhắn mới";
        var preview = notification.MessagePreview.Length <= PreviewMaxLength
            ? notification.MessagePreview
            : notification.MessagePreview[..PreviewMaxLength] + "…";
        var body = string.IsNullOrWhiteSpace(preview)
            ? $"{notification.SenderDisplayName} đã gửi tin nhắn cho bạn"
            : $"{notification.SenderDisplayName}: {preview}";

        var notificationEntity = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = notification.ReceiverId,
            SenderId = notification.SenderId,
            NotificationType = NotificationType.Chat,
            Title = title,
            Message = body,
            DeepLink = deepLink,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notificationEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var preference = await _notificationPreferenceRepository
            .GetByUserIdAsync(notification.ReceiverId, cancellationToken);

        if (preference is { Chat: false })
        {
            _logger.LogInformation(
                "User {ReceiverId} disabled Chat notifications. Skipping push.",
                notification.ReceiverId);
            return;
        }

        var data = new Dictionary<string, string>
        {
            { "type", "private_message" },
            { "messageId", notification.MessageId.ToString() },
            { "senderId", notification.SenderId.ToString() },
            { "chatId", chatId },
            { "chatName", notification.SenderDisplayName },
            { "deepLink", deepLink }
        };

        try
        {
            await _pushNotificationService.SendToUserAsync(
                notification.ReceiverId,
                title,
                body,
                data,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send private message push to user {ReceiverId}", notification.ReceiverId);
        }
    }
}
