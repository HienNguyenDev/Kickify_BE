using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public class AddFieldCommandValidator : AbstractValidator<AddFieldCommand>
    {
        public AddFieldCommandValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty().WithMessage("VenueId is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.FieldType)
                .NotEmpty().WithMessage("FieldType is required");

            

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("HourlyRate must be greater than 0");

            RuleFor(x => x.WeekendSurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("WeekendSurcharge must be greater than or equal to 0");

            RuleFor(x => x.HolidaySurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("HolidaySurcharge must be greater than or equal to 0");

            RuleForEach(x => x.PeakHours!).ChildRules(peak =>
            {
                peak.RuleFor(p => p.StartTime)
                    .NotEmpty().WithMessage("Peak hour StartTime is required")
                    .Must(value => TimeSpan.TryParse(value, out _))
                    .WithMessage("Peak hour StartTime must be in HH:mm:ss format");

                peak.RuleFor(p => p.EndTime)
                    .NotEmpty().WithMessage("Peak hour EndTime is required")
                    .Must(value => TimeSpan.TryParse(value, out _))
                    .WithMessage("Peak hour EndTime must be in HH:mm:ss format");

                peak.RuleFor(p => p)
                    .Must(p => TimeSpan.TryParse(p.StartTime, out var start) &&
                               TimeSpan.TryParse(p.EndTime, out var end) &&
                               start < end)
                    .WithMessage("Peak hour StartTime must be earlier than EndTime");

                peak.RuleFor(p => p.SurchargeAmount)
                    .GreaterThanOrEqualTo(0).WithMessage("Peak hour surcharge must be greater than or equal to 0");

                peak.RuleFor(p => p.ApplicableDays)
                    .NotEmpty().WithMessage("Peak hour ApplicableDays is required");

                peak.RuleFor(p => p.ApplicableDays)
                    .Must(days => days.Distinct(StringComparer.OrdinalIgnoreCase).Count() == days.Count)
                    .WithMessage("Peak hour ApplicableDays must not contain duplicate values");

                peak.RuleForEach(p => p.ApplicableDays)
                    .Must(day => Enum.TryParse<Kickify.Domain.Enums.DayOfWeekEnum>(day, true, out _))
                    .WithMessage("Peak hour ApplicableDays contains invalid day value");
            }).When(x => x.PeakHours is { Count: > 0 });
        }
    }
}
