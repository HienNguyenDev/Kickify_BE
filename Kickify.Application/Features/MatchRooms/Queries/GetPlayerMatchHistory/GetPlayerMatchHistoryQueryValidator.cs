using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Queries.GetPlayerMatchHistory
{
    public class GetPlayerMatchHistoryQueryValidator : AbstractValidator<GetPlayerMatchHistoryQuery>
    {
        public GetPlayerMatchHistoryQueryValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty().WithMessage("Target user id is required.");

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize must be less than or equal to 100.");
        }
    }
}
