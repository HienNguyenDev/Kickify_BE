using FluentValidation;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommandValidator : AbstractValidator<CreateMatchFeedbackCommand>
{
    public CreateMatchFeedbackCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty()
            .WithMessage("MatchId is required");

        RuleFor(x => x.RevieweeId)
            .NotEmpty()
            .WithMessage("RevieweeId is required");

        RuleFor(x => x.Feedbacks)
            .NotEmpty()
            .WithMessage("Feedbacks list must not be empty");

        RuleForEach(x => x.Feedbacks).ChildRules(feedback =>
        {
            feedback.RuleFor(f => f.ReviewerId)
                .NotEmpty()
                .WithMessage("ReviewerId is required");

            feedback.RuleFor(f => f.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");

            feedback.RuleFor(f => f.Comment)
                .MaximumLength(1000)
                .WithMessage("Comment must not exceed 1000 characters");
        });
    }
}
