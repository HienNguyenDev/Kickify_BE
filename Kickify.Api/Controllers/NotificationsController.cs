using Kickify.Api.Extensions;
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
}
