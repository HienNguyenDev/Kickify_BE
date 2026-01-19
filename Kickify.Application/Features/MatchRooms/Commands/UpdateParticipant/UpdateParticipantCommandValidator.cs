using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public class UpdateParticipantCommandValidator : AbstractValidator<UpdateParticipantCommand>
    {
        public UpdateParticipantCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");

            RuleFor(x => x.TeamAssignment)
                .Must(t => string.IsNullOrEmpty(t) || t == "A" || t == "B" || t == "Unassigned")
                .When(x => !string.IsNullOrEmpty(x.TeamAssignment))
                .WithMessage("Team assignment must be TeamA, TeamB, or Unassigned");
        }
    }
}
