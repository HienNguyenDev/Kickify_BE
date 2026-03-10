using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Announcements.Commands.UpdateAnnouncement;

public class AnnouncementUpdatedEventHandler : INotificationHandler<AnnouncementUpdatedDomainEvent>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<AnnouncementUpdatedEventHandler> _logger;

    public AnnouncementUpdatedEventHandler(
        IPushNotificationService pushNotificationService,
        ILogger<AnnouncementUpdatedEventHandler> logger)
    {
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task Handle(AnnouncementUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AnnouncementUpdatedEventHandler triggered. AnnouncementId: {AnnouncementId}, Title: {Title}",
            notification.AnnouncementId,
            notification.Title);

        var data = new Dictionary<string, string>
        {
            { "type", "announcement_updated" },
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
                "Push notification sent to topic 'all_users' for updated announcement {AnnouncementId}",
                notification.AnnouncementId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send push notification for updated announcement {AnnouncementId}",
                notification.AnnouncementId);
        }
    }
}
