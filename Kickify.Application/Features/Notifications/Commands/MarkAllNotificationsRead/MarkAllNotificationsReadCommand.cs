using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : ICommand<MarkAllNotificationsReadResponse>;
