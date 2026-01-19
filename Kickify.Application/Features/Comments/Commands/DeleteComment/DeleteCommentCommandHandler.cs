using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, DeleteCommentCommandResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public DeleteCommentCommandHandler(ICommentRepository commentRepository, IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<DeleteCommentCommandResponse>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment is null || !comment.IsActive)
        {
            return Result.Failure<DeleteCommentCommandResponse>(CommentErrors.NotFound(request.CommentId));
        }
        if (comment.UserId != _userContext.UserId)
        {
            return Result.Failure<DeleteCommentCommandResponse>(CommentErrors.Unauthorized);
        }

        var post = await _postRepository.GetByIdAsync(comment.PostId);
        if (post is not null)
        {
            post.TotalComments--;
            _postRepository.Update(post);
        }

        if (comment.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(comment.ParentCommentId.Value);
            if (parentComment is not null)
            {
                parentComment.TotalReplies--;
                _commentRepository.Update(parentComment);
            }
        }

        comment.IsActive = false;
        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new DeleteCommentCommandResponse
        {
            CommentId = comment.CommentId,
            DeletedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
