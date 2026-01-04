using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class ChatMessage 
{
    public Guid MessageId { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.Text;
    public bool IsEdited { get; set; } = false;
    public DateTime SentAt { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
