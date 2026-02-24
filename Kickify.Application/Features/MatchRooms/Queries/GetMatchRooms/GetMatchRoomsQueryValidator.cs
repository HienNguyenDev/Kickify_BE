using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public class GetMatchRoomsQueryValidator : AbstractValidator<GetMatchRoomsQuery>
    {
        public GetMatchRoomsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100");

            RuleFor(x => x.MatchFormat)
                .Must(f => string.IsNullOrEmpty(f) || f == "FiveVsFive" || f == "SevenVsSeven" || f == "ElevenVsEleven")
                .When(x => !string.IsNullOrEmpty(x.MatchFormat))
                .WithMessage("Match format must be FiveVsFive, SevenVsSeven, or ElevenVsEleven");
        }
    }
}
