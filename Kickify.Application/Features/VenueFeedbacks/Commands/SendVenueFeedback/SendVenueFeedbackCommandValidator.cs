using FluentValidation;

namespace Kickify.Application.Features.VenueFeedbacks.Commands.SendVenueFeedback;

public class SendVenueFeedbackCommandValidator : AbstractValidator<SendVenueFeedbackCommand>
{
    public SendVenueFeedbackCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }
}
