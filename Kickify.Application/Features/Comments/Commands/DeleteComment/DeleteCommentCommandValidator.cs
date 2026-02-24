using FluentValidation;

namespace Kickify.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required");
    }
}
