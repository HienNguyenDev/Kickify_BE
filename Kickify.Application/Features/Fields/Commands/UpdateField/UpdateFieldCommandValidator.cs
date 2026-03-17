using FluentValidation;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public class UpdateFieldCommandValidator : AbstractValidator<UpdateFieldCommand>
    {
        public UpdateFieldCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("FieldId is required");

            When(x => !string.IsNullOrEmpty(x.FieldName), () =>
            {
                RuleFor(x => x.FieldName)
                    .MaximumLength(100)
                    .WithMessage("FieldName must not exceed 100 characters");
            });

            When(x => x.HourlyRate.HasValue, () =>
            {
                RuleFor(x => x.HourlyRate)
                    .GreaterThan(0)
                    .WithMessage("HourlyRate must be greater than 0");
            });

            When(x => x.PeakHourSurcharge.HasValue, () =>
            {
                RuleFor(x => x.PeakHourSurcharge)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("PeakHourSurcharge must be greater than or equal to 0");
            });

            When(x => x.WeekendSurcharge.HasValue, () =>
            {
                RuleFor(x => x.WeekendSurcharge)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("WeekendSurcharge must be greater than or equal to 0");
            });

            When(x => x.HolidaySurcharge.HasValue, () =>
            {
                RuleFor(x => x.HolidaySurcharge)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("HolidaySurcharge must be greater than or equal to 0");
            });

            RuleFor(x => x)
                .Must(x => x.PeakStartTime.HasValue == x.PeakEndTime.HasValue || (!x.PeakStartTime.HasValue && !x.PeakEndTime.HasValue))
                .WithMessage("PeakStartTime and PeakEndTime must either both be provided or both be empty");

            RuleFor(x => x)
                .Must(x => !x.PeakStartTime.HasValue || !x.PeakEndTime.HasValue || x.PeakStartTime.Value < x.PeakEndTime.Value)
                .WithMessage("PeakStartTime must be earlier than PeakEndTime");

            When(x => x.PeakDaysOfWeek != null, () =>
            {
                RuleFor(x => x.PeakDaysOfWeek)
                    .Must(days => days != null && days.Distinct().Count() == days.Count)
                    .WithMessage("PeakDaysOfWeek must not contain duplicate values");

                RuleForEach(x => x.PeakDaysOfWeek!)
                    .IsInEnum()
                    .WithMessage("PeakDaysOfWeek contains an invalid day");
            });
        }
    }
}
