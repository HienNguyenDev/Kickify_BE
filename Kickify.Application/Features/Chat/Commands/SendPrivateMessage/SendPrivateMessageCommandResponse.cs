namespace Kickify.Application.Features.Chat.Commands.SendPrivateMessage;

public class SendPrivateMessageCommandResponse
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
