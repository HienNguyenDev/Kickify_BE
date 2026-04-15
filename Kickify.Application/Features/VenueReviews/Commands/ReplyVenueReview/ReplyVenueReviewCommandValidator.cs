using FluentValidation;

namespace Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;

public class ReplyVenueReviewCommandValidator : AbstractValidator<ReplyVenueReviewCommand>
{
    public ReplyVenueReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Reply must not be empty.")
            .MaximumLength(2000)
            .WithMessage("Reply cannot exceed 2000 characters");
    }
}
