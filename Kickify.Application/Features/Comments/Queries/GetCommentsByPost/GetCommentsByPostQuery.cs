using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQuery : IQuery<GetCommentsByPostQueryResponse>
{
    public Guid PostId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
