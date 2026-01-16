using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQuery : IQuery<GetPostByIdQueryResponse>
{
    public Guid PostId { get; set; }
}
