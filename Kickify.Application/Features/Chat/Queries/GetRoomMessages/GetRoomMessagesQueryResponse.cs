using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Chat.Queries.GetRoomMessages;

public class GetRoomMessagesQueryResponse
{
    public Guid RoomId { get; set; }
    public RoomChatChannel Channel { get; set; }
    public IEnumerable<RoomMessageDto> Messages { get; set; } = new List<RoomMessageDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class RoomMessageDto
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderFullName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public TeamAssignment? SenderTeam { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
}
