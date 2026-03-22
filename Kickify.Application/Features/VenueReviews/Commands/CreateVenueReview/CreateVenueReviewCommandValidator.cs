using FluentValidation;

namespace Kickify.Application.Features.VenueReviews.Commands.CreateVenueReview;

public class CreateVenueReviewCommandValidator : AbstractValidator<CreateVenueReviewCommand>
{
    public CreateVenueReviewCommandValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty().WithMessage("VenueId is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters")
            .When(x => x.Comment is not null);
    }
}
