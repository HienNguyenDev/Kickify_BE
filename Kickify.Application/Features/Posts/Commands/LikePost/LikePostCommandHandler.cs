using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Commands.LikePost;

public class LikePostCommandHandler : ICommandHandler<LikePostCommand, LikePostCommandResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeRepository _postLikeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public LikePostCommandHandler(
        IPostRepository postRepository,
        IPostLikeRepository postLikeRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _postRepository = postRepository;
        _postLikeRepository = postLikeRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<LikePostCommandResponse>> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<LikePostCommandResponse>(PostErrors.NotFound(request.PostId));
        }

        var existingLike = await _postLikeRepository.GetByPostAndUserAsync(request.PostId, _userContext.UserId);
        bool isLiked;

        if (existingLike is not null)
        {
            // Unlike
            _postLikeRepository.Remove(existingLike);
            post.TotalLikes = Math.Max(0, post.TotalLikes - 1);
            isLiked = false;
        }
        else
        {
            // Like
            var postLike = new PostLike
            {
                LikeId = Guid.NewGuid(),
                PostId = request.PostId,
                UserId = _userContext.UserId,
                CreatedAt = DateTime.UtcNow
            };
            await _postLikeRepository.AddAsync(postLike);
            post.TotalLikes++;
            isLiked = true;
        }

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LikePostCommandResponse
        {
            PostId = post.PostId,
            IsLiked = isLiked,
            TotalLikes = post.TotalLikes
        };

        return Result.Success(response);
    }
}
