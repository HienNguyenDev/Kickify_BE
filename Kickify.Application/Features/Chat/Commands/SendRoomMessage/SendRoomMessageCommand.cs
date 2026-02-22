using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Chat.Commands.SendRoomMessage;

public class SendRoomMessageCommand : ICommand<SendRoomMessageCommandResponse>
{
    public Guid? SenderId { get; set; }
    public Guid RoomId { get; set; }
    public RoomChatChannel Channel { get; set; }
    public string MessageText { get; set; } = string.Empty;
}
