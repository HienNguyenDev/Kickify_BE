using FluentValidation;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.RespondToFeedback;

public class RespondToFeedbackCommandValidator : AbstractValidator<RespondToFeedbackCommand>
{
    public RespondToFeedbackCommandValidator()
    {
        RuleFor(x => x.FeedbackId).NotEmpty();
        RuleFor(x => x.Response).NotEmpty().MaximumLength(1000);
    }
}
