namespace Kickify.Application.Features.Chat.Queries.GetPrivateConversation;

public class GetPrivateConversationQueryResponse
{
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatarUrl { get; set; }
    public IEnumerable<ConversationMessageDto> Messages { get; set; } = new List<ConversationMessageDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ConversationMessageDto
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsEdited { get; set; }
    public bool IsMine { get; set; }
    public DateTime SentAt { get; set; }
}
