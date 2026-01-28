using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Chat.Queries.GetConversationList;

public class GetConversationListQueryHandler : IQueryHandler<GetConversationListQuery, GetConversationListQueryResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserContext _userContext;

    public GetConversationListQueryHandler(IChatMessageRepository chatMessageRepository, IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetConversationListQueryResponse>> Handle(GetConversationListQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _chatMessageRepository.GetConversationListAsync(
            _userContext.UserId,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var total = await _chatMessageRepository.GetConversationCountAsync(
            _userContext.UserId,
            request.SearchTerm,
            cancellationToken);

        var conversationDtos = conversations.Select(c => new ConversationItemDto
        {
            OtherUserId = c.OtherUser.UserId,
            OtherUserName = c.OtherUser.FullName ?? string.Empty,
            OtherUserAvatarUrl = c.OtherUser.AvatarUrl,
            LastMessage = c.LastMessage.MessageText,
            LastMessageAt = c.LastMessage.SentAt,
            UnreadCount = c.UnreadCount,
            IsOnline = false
        }).ToList();

        var response = new GetConversationListQueryResponse
        {
            Conversations = conversationDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
