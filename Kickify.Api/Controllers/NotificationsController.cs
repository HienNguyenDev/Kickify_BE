using Kickify.Api.Extensions;
using Kickify.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using Kickify.Application.Features.Notifications.Commands.MarkNotificationRead;
using Kickify.Application.Features.Notifications.Queries.GetMyNotifications;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get notifications of current user with optional filter by type
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetMyNotifications(
        [FromQuery] NotificationType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyNotificationsQuery(type, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IResult> MarkNotificationRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var command = new MarkNotificationReadCommand(notificationId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Mark all notifications of current user as read.
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IResult> MarkAllNotificationsRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllNotificationsReadCommand();
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
