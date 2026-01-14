using FluentValidation;

namespace Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile
{
    public class DeletePlayerProfileCommandValidator : AbstractValidator<DeletePlayerProfileCommand>
    {
        public DeletePlayerProfileCommandValidator()
        {
            RuleFor(x => x.ProfileId)
                .NotEmpty().WithMessage("Profile ID is required");
        }
    }
}
