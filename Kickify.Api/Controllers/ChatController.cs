using Kickify.Api.Extensions;
using Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;
using Kickify.Application.Features.Chat.Commands.SendPrivateMessage;
using Kickify.Application.Features.Chat.Queries.GetConversationList;
using Kickify.Application.Features.Chat.Queries.GetPrivateConversation;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ISender _mediator;

    public ChatController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversations")]
    public async Task<IResult> GetConversationList(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetConversationListQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };
        Result<GetConversationListQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("conversation/{otherUserId:guid}")]
    public async Task<IResult> GetConversation(Guid otherUserId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new GetPrivateConversationQuery
        {
            OtherUserId = otherUserId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        Result<GetPrivateConversationQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("send")]
    public async Task<IResult> SendPrivateMessage([FromBody] SendPrivateMessageCommand command, CancellationToken cancellationToken)
    {
        Result<SendPrivateMessageCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("read/{fromUserId:guid}")]
    public async Task<IResult> MarkAsRead(Guid fromUserId, CancellationToken cancellationToken)
    {
        var command = new MarkMessagesAsReadCommand { FromUserId = fromUserId };
        Result<MarkMessagesAsReadCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
