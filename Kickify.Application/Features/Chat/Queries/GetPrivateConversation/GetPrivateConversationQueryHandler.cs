using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Chat.Queries.GetPrivateConversation;

public class GetPrivateConversationQueryHandler : IQueryHandler<GetPrivateConversationQuery, GetPrivateConversationQueryResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetPrivateConversationQueryHandler(
        IChatMessageRepository chatMessageRepository,
        IUserRepository userRepository,
        IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetPrivateConversationQueryResponse>> Handle(GetPrivateConversationQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = request.CurrentUserId ?? _userContext.UserId;

        var otherUser = await _userRepository.GetByIdAsync(request.OtherUserId);
        if (otherUser is null)
        {
            return Result.Failure<GetPrivateConversationQueryResponse>(ChatErrors.ReceiverNotFound);
        }

        var (messages, total) = await _chatMessageRepository.GetPrivateConversationAsync(
            currentUserId,
            request.OtherUserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var messageDtos = messages.Select(m => new ConversationMessageDto
        {
            MessageId = m.MessageId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName ?? string.Empty,
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            MessageText = m.MessageText,
            MessageType = m.MessageType.ToString(),
            IsRead = m.IsRead,
            IsEdited = m.IsEdited,
            IsMine = m.SenderId == currentUserId,
            SentAt = m.SentAt
        }).ToList();

        var response = new GetPrivateConversationQueryResponse
        {
            OtherUserId = otherUser.UserId,
            OtherUserName = otherUser.FullName ?? string.Empty,
            OtherUserAvatarUrl = otherUser.AvatarUrl,
            Messages = messageDtos,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
