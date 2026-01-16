using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommand : ICommand<MarkMessagesAsReadCommandResponse>
{
    public Guid? CurrentUserId { get; set; }
    public Guid FromUserId { get; set; }
}
