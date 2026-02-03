using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.KickPlayer
{
    public class KickPlayerCommandValidator : AbstractValidator<KickPlayerCommand>
    {
        public KickPlayerCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");

            RuleFor(x => x.TargetUserId)
                .NotEmpty().WithMessage("Target user ID is required");
        }
    }
}
