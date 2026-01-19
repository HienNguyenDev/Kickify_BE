using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CreateCommentCommandResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateCommentCommandHandler(ICommentRepository commentRepository, IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CreateCommentCommandResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<CreateCommentCommandResponse>(CommentErrors.PostNotFound);
        }

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
            if (parentComment is null)
            {
                return Result.Failure<CreateCommentCommandResponse>(CommentErrors.ParentCommentNotFound);
            }
            parentComment.TotalReplies++;
            _commentRepository.Update(parentComment);
        }

        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            PostId = request.PostId,
            UserId = _userContext.UserId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content
        };

        post.TotalComments++;
        _postRepository.Update(post);
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateCommentCommandResponse
        {
            CommentId = comment.CommentId,
            PostId = comment.PostId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };

        return Result.Success(response);
    }
}
