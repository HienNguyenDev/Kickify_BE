using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, UpdatePostCommandResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdatePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<UpdatePostCommandResponse>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<UpdatePostCommandResponse>(PostErrors.NotFound(request.PostId));
        }

        if (post.UserId != _userContext.UserId)
        {
            return Result.Failure<UpdatePostCommandResponse>(PostErrors.Unauthorized);
        }

        post.Content = request.Content;
        post.IsEdited = true;
        post.EditedAt = DateTime.UtcNow;

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdatePostCommandResponse
        {
            PostId = post.PostId,
            Content = post.Content,
            UpdatedAt = post.EditedAt.Value
        };

        return Result.Success(response);
    }
}
