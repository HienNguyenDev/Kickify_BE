using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

internal sealed class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public MarkAllNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result<MarkAllNotificationsReadResponse>> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(_userContext.UserId, cancellationToken);

        return Result.Success(new MarkAllNotificationsReadResponse(0));
    }
}
