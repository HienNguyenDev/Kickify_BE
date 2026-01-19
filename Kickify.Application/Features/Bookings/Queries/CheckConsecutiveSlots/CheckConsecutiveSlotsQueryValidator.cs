using FluentValidation;

namespace Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots
{
    public class CheckConsecutiveSlotsQueryValidator : AbstractValidator<CheckConsecutiveSlotsQuery>
    {
        public CheckConsecutiveSlotsQueryValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("FieldId is required");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .Must(date => date.Date >= DateTime.UtcNow.Date)
                .WithMessage("Date must be today or in the future");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime is required")
                .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
                .WithMessage("StartTime must be between 00:00 and 23:59");

            RuleFor(x => x.DurationMinutes)
                .NotEmpty().WithMessage("DurationMinutes is required")
                .Must(duration => duration == 60 || duration == 90 || duration == 120)
                .WithMessage("DurationMinutes must be 60, 90, or 120");
        }
    }
}
