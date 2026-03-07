namespace Kickify.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsResponse(
    List<NotificationDto> Notifications,
    int Total,
    int Page,
    int PageSize,
    int TotalPages);

public record NotificationDto(
    Guid NotificationId,
    string NotificationType,
    string Title,
    string Message,
    string? DeepLink,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);
