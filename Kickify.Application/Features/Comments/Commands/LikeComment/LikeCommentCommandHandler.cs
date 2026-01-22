using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Commands.LikeComment;

public class LikeCommentCommandHandler : ICommandHandler<LikeCommentCommand, LikeCommentCommandResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public LikeCommentCommandHandler(ICommentRepository commentRepository, ICommentLikeRepository commentLikeRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<LikeCommentCommandResponse>> Handle(LikeCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment is null || !comment.IsActive) 
        {
            return Result.Failure<LikeCommentCommandResponse>(CommentErrors.NotFound(request.CommentId));
        }

        var existingLike = await _commentLikeRepository.GetByCommentAndUserAsync(request.CommentId, _userContext.UserId, cancellationToken);
        bool isLiked;

        if (existingLike is not null)
        {
            _commentLikeRepository.Remove(existingLike);
            comment.TotalLikes--;
            isLiked = false;
        }
        else
        {
            var commentLike = new CommentLike 
            { 
                LikeId = Guid.NewGuid(), 
                CommentId = request.CommentId, 
                UserId = _userContext.UserId 
            };
            await _commentLikeRepository.AddAsync(commentLike);
            comment.TotalLikes++;
            isLiked = true;
        }

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LikeCommentCommandResponse
        {
            CommentId = comment.CommentId,
            IsLiked = isLiked,
            TotalLikes = comment.TotalLikes
        };

        return Result.Success(response);
    }
}
