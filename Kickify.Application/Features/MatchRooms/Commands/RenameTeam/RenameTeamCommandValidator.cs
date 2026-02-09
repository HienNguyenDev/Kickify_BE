using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.RenameTeam
{
    public class RenameTeamCommandValidator : AbstractValidator<RenameTeamCommand>
    {
        public RenameTeamCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("RoomId is required");

            RuleFor(x => x.Team)
                .NotEmpty().WithMessage("Team is required")
                .Must(t => t == "A" || t == "B").WithMessage("Team must be A or B");

            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Team name must not exceed 50 characters")
                .When(x => x.Name != null);
        }
    }
}
