namespace Kickify.Application.Features.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadResponse(Guid NotificationId, bool IsRead, DateTime? ReadAt);
