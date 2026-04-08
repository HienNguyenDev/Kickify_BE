using FluentValidation;

namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyReceivedFeedbacks;

public class GetMyReceivedFeedbacksQueryValidator : AbstractValidator<GetMyReceivedFeedbacksQuery>
{
    public GetMyReceivedFeedbacksQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);
    }
}
