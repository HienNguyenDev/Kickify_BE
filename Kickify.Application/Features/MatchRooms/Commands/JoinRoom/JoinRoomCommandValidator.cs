using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
    {
        public JoinRoomCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");
        }
    }
}
