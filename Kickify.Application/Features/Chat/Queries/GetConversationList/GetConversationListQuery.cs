using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Chat.Queries.GetConversationList;

public class GetConversationListQuery : IQuery<GetConversationListQueryResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
