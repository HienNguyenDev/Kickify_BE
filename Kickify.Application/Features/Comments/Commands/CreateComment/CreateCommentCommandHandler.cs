using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, CreateCommentCommandResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IPublisher _publisher;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IPublisher publisher)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _publisher = publisher;
    }

    public async Task<Result<CreateCommentCommandResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<CreateCommentCommandResponse>(CommentErrors.PostNotFound);
        }

        Comment? parentComment = null;
        if (request.ParentCommentId.HasValue)
        {
            parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
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

        var recipientUserId = parentComment is not null ? parentComment.UserId : post.UserId;
        var isReply = parentComment is not null;
        if (recipientUserId != _userContext.UserId)
        {
            var actor = await _userRepository.GetByIdAsync(_userContext.UserId);
            if (actor is not null)
            {
                await _publisher.Publish(
                    new PostCommentCreatedDomainEvent(
                        request.PostId,
                        comment.CommentId,
                        isReply,
                        recipientUserId,
                        _userContext.UserId,
                        actor.FullName ?? actor.Email),
                    cancellationToken);
            }
        }

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
