using Kickify.Api.Extensions;
using Kickify.Application.Features.Friendships.Commands.RemoveFriend;
using Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;
using Kickify.Application.Features.Friendships.Commands.SendFriendRequest;
using Kickify.Application.Features.Friendships.Queries.GetFriends;
using Kickify.Application.Features.Friendships.Queries.GetPendingRequests;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/friendships")]
[ApiController]
[Authorize]
public class FriendshipsController : ControllerBase
{
    private readonly ISender _mediator;

    public FriendshipsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("friends")]
    public async Task<IResult> GetFriends([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetFriendsQuery { Page = page, PageSize = pageSize };
        Result<GetFriendsQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("requests")]
    public async Task<IResult> GetPendingRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetPendingRequestsQuery { Page = page, PageSize = pageSize };
        Result<GetPendingRequestsQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("request/{addresseeId:guid}")]
    public async Task<IResult> SendFriendRequest(Guid addresseeId, CancellationToken cancellationToken)
    {
        var command = new SendFriendRequestCommand { AddresseeId = addresseeId };
        Result<SendFriendRequestCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("respond/{friendshipId:guid}")]
    public async Task<IResult> RespondFriendRequest(Guid friendshipId, [FromBody] RespondFriendRequestRequest request, CancellationToken cancellationToken)
    {
        var command = new RespondFriendRequestCommand { FriendshipId = friendshipId, Accept = request.Accept };
        Result<RespondFriendRequestCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{friendId:guid}")]
    public async Task<IResult> RemoveFriend(Guid friendId, CancellationToken cancellationToken)
    {
        var command = new RemoveFriendCommand { FriendId = friendId };
        Result<RemoveFriendCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

public class RespondFriendRequestRequest
{
    public bool Accept { get; set; }
}
