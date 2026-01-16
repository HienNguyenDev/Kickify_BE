namespace Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandResponse
{
    public Guid FromUserId { get; set; }
    public bool Success { get; set; }
}
