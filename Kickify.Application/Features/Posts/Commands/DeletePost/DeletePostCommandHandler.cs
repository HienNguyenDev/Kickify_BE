using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : ICommandHandler<DeletePostCommand, DeletePostCommandResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public DeletePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<DeletePostCommandResponse>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<DeletePostCommandResponse>(PostErrors.NotFound(request.PostId));
        }

        if (post.UserId != _userContext.UserId)
        {
            return Result.Failure<DeletePostCommandResponse>(PostErrors.Unauthorized);
        }

        _postRepository.Remove(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new DeletePostCommandResponse
        {
            PostId = post.PostId,
            DeletedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
