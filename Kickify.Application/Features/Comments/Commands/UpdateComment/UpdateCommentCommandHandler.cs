using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand, UpdateCommentCommandResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdateCommentCommandHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<UpdateCommentCommandResponse>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment is null || !comment.IsActive)
        {
            return Result.Failure<UpdateCommentCommandResponse>(CommentErrors.NotFound(request.CommentId));
        }
        if (comment.UserId != _userContext.UserId)
        {
            return Result.Failure<UpdateCommentCommandResponse>(CommentErrors.Unauthorized);
        }

        comment.Content = request.Content;
        comment.IsEdited = true;
        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateCommentCommandResponse
        {
            CommentId = comment.CommentId,
            Content = comment.Content,
            UpdatedAt = comment.UpdatedAt
        };

        return Result.Success(response);
    }
}
