using FluentValidation;

namespace Kickify.Application.Features.PlayerProfiles.Commands.UpdatePlayerProfile
{
    public class UpdatePlayerProfileCommandValidator : AbstractValidator<UpdatePlayerProfileCommand>
    {
        public UpdatePlayerProfileCommandValidator()
        {
            RuleFor(x => x.ProfileId)
                .NotEmpty().WithMessage("Profile ID is required");

            RuleFor(x => x.CurrentElo)
                .GreaterThanOrEqualTo(0).WithMessage("ELO rating cannot be negative")
                .LessThanOrEqualTo(5000).WithMessage("ELO rating cannot exceed 5000")
                .When(x => x.CurrentElo.HasValue);

            RuleFor(x => x.TrustScore)
                .GreaterThanOrEqualTo(0).WithMessage("Trust score cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Trust score cannot exceed 100")
                .When(x => x.TrustScore.HasValue);

            RuleFor(x => x.TotalMatches)
                .GreaterThanOrEqualTo(0).WithMessage("Total matches cannot be negative")
                .When(x => x.TotalMatches.HasValue);

            RuleFor(x => x.Wins)
                .GreaterThanOrEqualTo(0).WithMessage("Wins cannot be negative")
                .When(x => x.Wins.HasValue);

            RuleFor(x => x.Losses)
                .GreaterThanOrEqualTo(0).WithMessage("Losses cannot be negative")
                .When(x => x.Losses.HasValue);

            RuleFor(x => x.Draws)
                .GreaterThanOrEqualTo(0).WithMessage("Draws cannot be negative")
                .When(x => x.Draws.HasValue);

            RuleFor(x => x.MvpCount)
                .GreaterThanOrEqualTo(0).WithMessage("MVP count cannot be negative")
                .When(x => x.MvpCount.HasValue);

            RuleFor(x => x.WinStreak)
                .GreaterThanOrEqualTo(0).WithMessage("Win streak cannot be negative")
                .When(x => x.WinStreak.HasValue);

            RuleFor(x => x.MaxWinStreak)
                .GreaterThanOrEqualTo(0).WithMessage("Max win streak cannot be negative")
                .When(x => x.MaxWinStreak.HasValue);

            RuleFor(x => x.AfkCount)
                .GreaterThanOrEqualTo(0).WithMessage("AFK count cannot be negative")
                .When(x => x.AfkCount.HasValue);

            RuleFor(x => x.ReportCount)
                .GreaterThanOrEqualTo(0).WithMessage("Report count cannot be negative")
                .When(x => x.ReportCount.HasValue);

            RuleFor(x => x.PreferredPositions)
                .MaximumLength(255).WithMessage("Preferred positions cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.PreferredPositions));
        }
    }
}
