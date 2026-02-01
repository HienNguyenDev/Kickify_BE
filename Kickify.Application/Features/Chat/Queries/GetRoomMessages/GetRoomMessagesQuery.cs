using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Chat.Queries.GetRoomMessages;

public class GetRoomMessagesQuery : IQuery<GetRoomMessagesQueryResponse>
{
    public Guid RoomId { get; set; }
    public RoomChatChannel Channel { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
