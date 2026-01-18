using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public class LeaveRoomCommandValidator : AbstractValidator<LeaveRoomCommand>
    {
        public LeaveRoomCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");
        }
    }
}
