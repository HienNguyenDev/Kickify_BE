using FluentValidation;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerProfileById
{
    public class GetPlayerProfileByIdQueryValidator : AbstractValidator<GetPlayerProfileByIdQuery>
    {
        public GetPlayerProfileByIdQueryValidator()
        {
            RuleFor(x => x.ProfileId)
                .NotEmpty().WithMessage("Profile ID is required");
        }
    }
}
