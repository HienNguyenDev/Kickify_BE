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

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.RadiusKm)
                .GreaterThan(0)
                .When(x => x.RadiusKm.HasValue)
                .WithMessage("Radius must be greater than 0");
        }
    }
}
