using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateFormation
{
    public class UpdateFormationCommandValidator : AbstractValidator<UpdateFormationCommand>
    {
        public UpdateFormationCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("RoomId is required");

            RuleFor(x => x.Team)
                .NotEmpty().WithMessage("Team is required")
                .Must(t => t == "A" || t == "B").WithMessage("Team must be A or B");

            RuleFor(x => x.FormationName)
                .NotEmpty().WithMessage("FormationName is required")
                .MaximumLength(20).WithMessage("FormationName must not exceed 20 characters");

            RuleFor(x => x.Assignments)
                .NotNull().WithMessage("Assignments is required");

            RuleForEach(x => x.Assignments)
                .ChildRules(assignment =>
                {
                    assignment.RuleFor(a => a.PlayerId)
                        .NotEmpty().WithMessage("PlayerId is required for each assignment");

                    assignment.RuleFor(a => a.SlotId)
                        .NotEmpty().WithMessage("SlotId is required for each assignment")
                        .Matches(@"^(GK|DF|MF|FW)-\d+$").WithMessage("SlotId must be in format: GK-0, DF-1, MF-2, FW-0");
                });
        }
    }
}
