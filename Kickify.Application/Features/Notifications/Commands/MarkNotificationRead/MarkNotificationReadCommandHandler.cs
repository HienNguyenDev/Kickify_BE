using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Notifications.Commands.MarkNotificationRead;

internal sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand, MarkNotificationReadResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MarkNotificationReadResponse>> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(
            request.NotificationId, _userContext.UserId, cancellationToken);

        if (notification is null)
            return Result.Failure<MarkNotificationReadResponse>(NotificationErrors.NotFound(request.NotificationId));

        if (notification.IsRead)
            return Result.Success(new MarkNotificationReadResponse(
                notification.NotificationId, notification.IsRead, notification.ReadAt));

        notification.IsRead = true;
        notification.ReadAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarkNotificationReadResponse(
            notification.NotificationId, notification.IsRead, notification.ReadAt));
    }
}
