using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId) : ICommand<MarkNotificationReadResponse>;
