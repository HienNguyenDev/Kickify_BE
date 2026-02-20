using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.NotificationPreferences.Commands.UpdateNotificationPreference;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/notification-preferences")]
[ApiController]
[Authorize]
public class NotificationPreferencesController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationPreferencesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    public async Task<IResult> UpdateNotificationPreference(
        [FromBody] UpdateNotificationPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNotificationPreferenceCommand
        {
            MatchRoom = request.MatchRoom,
            Friendship = request.Friendship,
            Post = request.Post
        };

        Result<UpdateNotificationPreferenceCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
