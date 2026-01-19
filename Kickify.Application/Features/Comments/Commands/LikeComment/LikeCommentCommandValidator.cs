using FluentValidation;

namespace Kickify.Application.Features.Comments.Commands.LikeComment;

public class LikeCommentCommandValidator : AbstractValidator<LikeCommentCommand>
{
    public LikeCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required");
    }
}
