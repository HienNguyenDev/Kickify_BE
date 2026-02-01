using Kickify.Api.Extensions;
using Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;
using Kickify.Application.Features.Chat.Commands.SendPrivateMessage;
using Kickify.Application.Features.Chat.Commands.SendRoomMessage;
using Kickify.Application.Features.Chat.Queries.GetConversationList;
using Kickify.Application.Features.Chat.Queries.GetPrivateConversation;
using Kickify.Application.Features.Chat.Queries.GetRoomMessages;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
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

    #region Private Chat

    /// <summary>
    /// Lấy danh sách cuộc trò chuyện private
    /// </summary>
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

    /// <summary>
    /// Lấy tin nhắn private với một user khác
    /// </summary>
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

    /// <summary>
    /// Gửi tin nhắn private
    /// </summary>
    [HttpPost("send")]
    public async Task<IResult> SendPrivateMessage([FromBody] SendPrivateMessageCommand command, CancellationToken cancellationToken)
    {
        Result<SendPrivateMessageCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Đánh dấu tin nhắn đã đọc
    /// </summary>
    [HttpPost("read/{fromUserId:guid}")]
    public async Task<IResult> MarkAsRead(Guid fromUserId, CancellationToken cancellationToken)
    {
        var command = new MarkMessagesAsReadCommand { FromUserId = fromUserId };
        Result<MarkMessagesAsReadCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    #endregion

    #region Room Chat

    /// <summary>
    /// Lấy tin nhắn trong room theo channel (General, TeamA, TeamB)
    /// </summary>
    [HttpGet("room/{roomId:guid}/messages")]
    public async Task<IResult> GetRoomMessages(
        Guid roomId,
        [FromQuery] RoomChatChannel channel = RoomChatChannel.General,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRoomMessagesQuery
        {
            RoomId = roomId,
            Channel = channel,
            Page = page,
            PageSize = pageSize
        };
        Result<GetRoomMessagesQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Gửi tin nhắn trong room (General: tất cả, TeamA: chỉ team A, TeamB: chỉ team B)
    /// </summary>
    [HttpPost("room/send")]
    public async Task<IResult> SendRoomMessage([FromBody] SendRoomMessageCommand command, CancellationToken cancellationToken)
    {
        Result<SendRoomMessageCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    #endregion
}
