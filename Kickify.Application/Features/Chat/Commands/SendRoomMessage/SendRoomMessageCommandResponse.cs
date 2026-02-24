using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Chat.Commands.SendRoomMessage;

public class SendRoomMessageCommandResponse
{
    public Guid MessageId { get; set; }
    public Guid RoomId { get; set; }
    public RoomChatChannel Channel { get; set; }
    public Guid SenderId { get; set; }
    public string SenderFullName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public TeamAssignment SenderTeam { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
