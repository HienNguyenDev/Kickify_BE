using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.SubmitAfkVote;

public class SubmitAfkVoteCommandValidator : AbstractValidator<SubmitAfkVoteCommand>
{
    public SubmitAfkVoteCommandValidator()
    {
        RuleFor(x => x.MatchRoomId)
            .NotEmpty();

        RuleFor(x => x.TargetPlayerIds)
            .NotNull()
            .Must(x => x.Count > 0)
            .WithMessage("At least one target player must be provided.");
    }
}
