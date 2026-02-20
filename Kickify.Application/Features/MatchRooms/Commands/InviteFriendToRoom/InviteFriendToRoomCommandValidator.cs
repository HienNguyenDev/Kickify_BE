using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.InviteFriendToRoom;

public class InviteFriendToRoomCommandValidator : AbstractValidator<InviteFriendToRoomCommand>
{
    public InviteFriendToRoomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required");

        RuleFor(x => x.FriendUserId)
            .NotEmpty().WithMessage("Friend User ID is required");
    }
}
