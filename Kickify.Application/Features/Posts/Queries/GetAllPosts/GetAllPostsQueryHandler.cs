using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Posts.Queries.GetAllPosts;

public class GetAllPostsQueryHandler : IQueryHandler<GetAllPostsQuery, GetAllPostsQueryResponse>
{
    private readonly IPostRepository _postRepository;

    public GetAllPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<GetAllPostsQueryResponse>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var (posts, total) = await _postRepository.GetPagedPostsAsync(
            userId: request.UserId,
            searchTerm: request.SearchTerm,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var postDtos = posts.Select(p => new PostDto
        {
            PostId = p.PostId,
            UserId = p.UserId,
            UserFullName = p.User?.FullName ?? string.Empty,
            UserAvatarUrl = p.User?.AvatarUrl,
            Content = p.Content,
            TotalMedia = p.TotalMedia,
            TotalLikes = p.TotalLikes,
            TotalComments = p.TotalComments,
            Visibility = p.Visibility,
            IsEdited = p.IsEdited,
            EditedAt = p.EditedAt,
            CreatedAt = p.CreatedAt,
            Media = p.PostMedia.Select(m => new PostMediaDto
            {
                MediaId = m.MediaId,
                PublicUrl = m.PublicUrl,
                MediaType = m.MediaType,
                DisplayOrder = m.DisplayOrder
            }).ToList()
        }).ToList();

        var response = new GetAllPostsQueryResponse
        {
            Posts = postDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
