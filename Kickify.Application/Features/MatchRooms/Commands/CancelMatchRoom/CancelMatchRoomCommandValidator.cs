using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.CancelMatchRoom;

public sealed class CancelMatchRoomCommandValidator : AbstractValidator<CancelMatchRoomCommand>
{
    public CancelMatchRoomCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
