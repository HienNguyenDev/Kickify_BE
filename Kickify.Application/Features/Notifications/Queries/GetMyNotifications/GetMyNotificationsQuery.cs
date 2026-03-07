using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(
    NotificationType? Type = null,
    int Page = 1,
    int PageSize = 20) : IQuery<GetMyNotificationsResponse>;
