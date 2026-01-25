using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Friendships.Commands.RemoveFriend;
using Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;
using Kickify.Application.Features.Friendships.Commands.SendFriendRequest;
using Kickify.Application.Features.Friendships.Queries.GetFriends;
using Kickify.Application.Features.Friendships.Queries.GetFriendshipStatus;
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

    [HttpGet("status/{userId:guid}")]
    public async Task<IResult> GetFriendshipStatus(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetFriendshipStatusQuery { OtherUserId = userId };
        Result<GetFriendshipStatusQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("request/{addresseeId:guid}")]
    public async Task<IResult> SendFriendRequest(Guid addresseeId, CancellationToken cancellationToken)
    {
        var command = new SendFriendRequestCommand { AddresseeId = addresseeId };
        Result<SendFriendRequestCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("respond/{requesterId:guid}")]
    public async Task<IResult> RespondFriendRequest(Guid requesterId, [FromBody] RespondFriendRequestRequest request, CancellationToken cancellationToken)
    {
        var command = new RespondFriendRequestCommand { RequesterId = requesterId, Accept = request.Accept };
        Result<RespondFriendRequestCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IResult> RemoveFriend(Guid userId, CancellationToken cancellationToken)
    {
        var command = new RemoveFriendCommand { FriendId = userId };
        Result<RemoveFriendCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}


