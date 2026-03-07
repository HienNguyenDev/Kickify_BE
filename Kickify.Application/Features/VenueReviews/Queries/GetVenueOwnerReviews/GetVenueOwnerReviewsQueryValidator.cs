using FluentValidation;

namespace Kickify.Application.Features.VenueReviews.Queries.GetVenueOwnerReviews;

public class GetVenueOwnerReviewsQueryValidator : AbstractValidator<GetVenueOwnerReviewsQuery>
{
    public GetVenueOwnerReviewsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("MinRating must be between 1 and 5");

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MaxRating.HasValue)
            .WithMessage("MaxRating must be between 1 and 5");
    }
}
