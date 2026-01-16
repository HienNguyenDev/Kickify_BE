using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Chat.Commands.SendPrivateMessage;

public class SendPrivateMessageCommand : ICommand<SendPrivateMessageCommandResponse>
{
    public Guid? SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.Text;
}
