using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.CreateVenue
{
    public class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
    {
        public CreateVenueCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

            RuleFor(x => x.Description)
                .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.IgnoredHolidayIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("IgnoredHolidayIds must not contain duplicate values");

            RuleForEach(x => x.Fields).ChildRules(field =>
            {
                field.RuleFor(f => f.Name)
                    .NotEmpty().WithMessage("Field name is required")
                    .MaximumLength(100).WithMessage("Field name must not exceed 100 characters");

                field.RuleFor(f => f.FieldType)
                    .NotEmpty().WithMessage("Field type is required");

                field.RuleFor(f => f.HourlyRate)
                    .GreaterThan(0).WithMessage("HourlyRate must be greater than 0");

                field.RuleFor(f => f.WeekendSurcharge)
                    .GreaterThanOrEqualTo(0).WithMessage("WeekendSurcharge must be greater than or equal to 0");

                field.RuleFor(f => f.HolidaySurcharge)
                    .GreaterThanOrEqualTo(0).WithMessage("HolidaySurcharge must be greater than or equal to 0");

                field.RuleForEach(f => f.PeakHours!).ChildRules(peak =>
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
                }).When(f => f.PeakHours is { Count: > 0 });
            });

            RuleFor(x => x.OperatingHours)
                .NotEmpty().WithMessage("At least one operating hour is required");

            RuleFor(x => x.OperatingHours)
                .Must(hours => hours.Select(h => h.DayOfWeek).Distinct().Count() == hours.Count)
                .WithMessage("OperatingHours must not contain duplicate DayOfWeek values");

            RuleForEach(x => x.OperatingHours).ChildRules(oh =>
            {
                oh.RuleFor(o => o.DayOfWeek)
                    .InclusiveBetween(0, 6)
                    .WithMessage("DayOfWeek must be between 0 and 6");

                oh.RuleFor(o => o)
                    .Must(o => o.IsClosed || (o.OpenTime.HasValue && o.CloseTime.HasValue && o.OpenTime.Value < o.CloseTime.Value))
                    .WithMessage("For open day, OpenTime and CloseTime are required and OpenTime must be before CloseTime");
            });
        }
    }
}
