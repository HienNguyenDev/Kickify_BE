using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.VoteMatchResult;

public class VoteMatchResultCommandValidator : AbstractValidator<VoteMatchResultCommand>
{
    public VoteMatchResultCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required");

        RuleFor(x => x.Vote)
            .IsInEnum().WithMessage("Invalid vote value. Must be TeamAWin, TeamBWin, or Draw");
    }
}
