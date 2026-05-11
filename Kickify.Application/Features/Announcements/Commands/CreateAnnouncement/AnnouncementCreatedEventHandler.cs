using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Event;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;

public class AnnouncementCreatedEventHandler : INotificationHandler<AnnouncementCreatedDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<AnnouncementCreatedEventHandler> _logger;

    public AnnouncementCreatedEventHandler(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotificationService,
        ILogger<AnnouncementCreatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(AnnouncementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AnnouncementCreatedEventHandler triggered. AnnouncementId: {AnnouncementId}, Title: {Title}",
            notification.AnnouncementId,
            notification.Title);

        var recipientIds = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.IsActive)
            .Select(user => user.UserId)
            .ToListAsync(cancellationToken);

        if (recipientIds.Count > 0)
        {
            var createdAt = DateTime.UtcNow;
            var deepLink = $"kickify://announcements/{notification.AnnouncementId}";
            var notifications = recipientIds.Select(userId => new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                SenderId = notification.CreatedBy,
                NotificationType = NotificationType.Announcement,
                Title = notification.Title,
                Message = notification.Content,
                DeepLink = deepLink,
                IsRead = false,
                CreatedAt = createdAt
            });

            await _dbContext.Notifications.AddRangeAsync(notifications, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Persisted {NotificationCount} announcement notifications for AnnouncementId: {AnnouncementId}",
                recipientIds.Count,
                notification.AnnouncementId);
        }

        var data = new Dictionary<string, string>
        {
            { "type", "announcement" },
            { "announcementId", notification.AnnouncementId.ToString() },
            { "announcementType", notification.AnnouncementType.ToString() }
        };

        try
        {
            await _pushNotificationService.SendToUsersAsync(
                recipientIds,
                notification.Title,
                notification.Content,
                data,
                cancellationToken);

            _logger.LogInformation(
                "Push notification sent to {RecipientCount} users for announcement {AnnouncementId}",
                recipientIds.Count,
                notification.AnnouncementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send push notification for announcement {AnnouncementId}",
                notification.AnnouncementId);
        }
    }
}
