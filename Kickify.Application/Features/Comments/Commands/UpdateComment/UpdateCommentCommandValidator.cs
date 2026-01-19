using FluentValidation;

namespace Kickify.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required").MaximumLength(2000).WithMessage("Content must not exceed 2000 characters");
    }
}
