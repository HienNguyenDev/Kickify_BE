using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.GenerateRoomInviteLink;

public class GenerateRoomInviteLinkCommandValidator : AbstractValidator<GenerateRoomInviteLinkCommand>
{
    public GenerateRoomInviteLinkCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required");
    }
}
