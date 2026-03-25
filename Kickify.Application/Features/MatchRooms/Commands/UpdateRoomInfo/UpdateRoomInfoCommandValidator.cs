using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomInfo
{
    public class UpdateRoomInfoCommandValidator : AbstractValidator<UpdateRoomInfoCommand>
    {
        public UpdateRoomInfoCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");

            RuleFor(x => x)
                .Must(x => x.RoomName != null || x.Description != null || x.Rules != null)
                .WithMessage("At least one field (RoomName, Description, Rules) must be provided");

            When(x => x.RoomName != null, () =>
            {
                RuleFor(x => x.RoomName)
                    .MaximumLength(100)
                    .WithMessage("Room name must not exceed 100 characters");
            });
        }
    }
}
