using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IQueryHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyNotificationsResponse>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var (notifications, total) = await _notificationRepository.GetByUserIdAsync(
            userId,
            request.Type,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto(
            n.NotificationId,
            n.NotificationType.ToString(),
            n.Title,
            n.Message,
            n.DeepLink,
            n.IsRead,
            n.ReadAt,
            n.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        return Result.Success(new GetMyNotificationsResponse(
            dtos, total, request.Page, request.PageSize, totalPages));
    }
}
