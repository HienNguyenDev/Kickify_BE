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

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
        }
    }
}
