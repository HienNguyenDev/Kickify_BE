using FluentValidation;

namespace Kickify.Application.Features.PlayerProfiles.Commands.CreatePlayerProfile
{
    public class CreatePlayerProfileCommandValidator : AbstractValidator<CreatePlayerProfileCommand>
    {
        public CreatePlayerProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.CurrentElo)
                .GreaterThanOrEqualTo(0).WithMessage("ELO rating cannot be negative")
                .LessThanOrEqualTo(5000).WithMessage("ELO rating cannot exceed 5000")
                .When(x => x.CurrentElo.HasValue);

            RuleFor(x => x.TrustScore)
                .GreaterThanOrEqualTo(0).WithMessage("Trust score cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Trust score cannot exceed 100")
                .When(x => x.TrustScore.HasValue);

            RuleFor(x => x.PreferredPositions)
                .MaximumLength(255).WithMessage("Preferred positions cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.PreferredPositions));
        }
    }
}
