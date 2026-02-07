using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public class CreateMatchRoomCommandValidator : AbstractValidator<CreateMatchRoomCommand>
    {
        public CreateMatchRoomCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("Field ID is required");

            RuleFor(x => x.MatchDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Match date cannot be in the past");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required");

            RuleFor(x => x.DurationMinutes)
                .Must(d => d == 60 || d == 90 || d == 120)
                .WithMessage("Duration must be 60, 90, or 120 minutes");

            RuleFor(x => x.MatchFormat)
                .NotEmpty().WithMessage("Match format is required")
                .Must(f => f == "FiveVsFive" || f == "SevenVsSeven" || f == "ElevenVsEleven")
                .WithMessage("Match format must be FiveVsFive, SevenVsSeven, or ElevenVsEleven");

            RuleFor(x => x.Visibility)
                .Must(v => string.IsNullOrEmpty(v) || v == "Public" || v == "Private")
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
