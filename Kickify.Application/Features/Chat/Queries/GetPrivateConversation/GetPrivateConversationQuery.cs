using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Chat.Queries.GetPrivateConversation;

public class GetPrivateConversationQuery : IQuery<GetPrivateConversationQueryResponse>
{
    public Guid? CurrentUserId { get; set; }
    public Guid OtherUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
