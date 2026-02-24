using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy
{
    public class UpdateRoomPrivacyCommandValidator : AbstractValidator<UpdateRoomPrivacyCommand>
    {
        public UpdateRoomPrivacyCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");

            RuleFor(x => x.Visibility)
                .NotEmpty().WithMessage("Visibility is required")
                .Must(v => v == "Public" || v == "Private")
                .WithMessage("Visibility must be Public or Private");

            RuleFor(x => x.Password)
                .MinimumLength(4).WithMessage("Room password must be at least 4 characters")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required for private rooms")
                .When(x => x.Visibility == "Private");
        }
    }
}
