using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Posts.Queries.GetAllPosts;

public class GetAllPostsQuery : IQuery<GetAllPostsQueryResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; }
}
