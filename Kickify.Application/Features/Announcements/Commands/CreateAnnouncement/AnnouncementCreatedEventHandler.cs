using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;

public class AnnouncementCreatedEventHandler : INotificationHandler<AnnouncementCreatedDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<AnnouncementCreatedEventHandler> _logger;

    public AnnouncementCreatedEventHandler(
        IPushNotificationService pushNotificationService,
        ILogger<AnnouncementCreatedEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(AnnouncementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AnnouncementCreatedEventHandler triggered. AnnouncementId: {AnnouncementId}, Title: {Title}",
            notification.AnnouncementId,
            notification.Title);

        var data = new Dictionary<string, string>
        {
            { "type", "announcement" },
            { "announcementId", notification.AnnouncementId.ToString() },
            { "announcementType", notification.AnnouncementType.ToString() }
        };

        try
        {
            await _pushNotificationService.SendToTopicAsync(
                "all_users",
                notification.Title,
                notification.Content,
                data,
                cancellationToken);

            _logger.LogInformation(
                "Push notification sent to topic 'all_users' for announcement {AnnouncementId}",
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
