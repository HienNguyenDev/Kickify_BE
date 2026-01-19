using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Queries.GetRepliesByComment;

public class GetRepliesByCommentQuery : IQuery<GetRepliesByCommentQueryResponse>
{
    public Guid CommentId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
