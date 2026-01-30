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

            RuleFor(x => x.DepositPerPerson)
                .GreaterThanOrEqualTo(0).When(x => x.DepositPerPerson.HasValue)
                .WithMessage("Deposit must be greater than or equal to 0");
        }
    }
}
