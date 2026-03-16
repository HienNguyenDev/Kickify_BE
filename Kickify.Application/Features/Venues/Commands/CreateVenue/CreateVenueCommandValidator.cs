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

                field.RuleFor(f => f.PeakHourSurcharge)
                    .GreaterThanOrEqualTo(0).WithMessage("PeakHourSurcharge must be greater than or equal to 0");

                field.RuleFor(f => f.WeekendSurcharge)
                    .GreaterThanOrEqualTo(0).WithMessage("WeekendSurcharge must be greater than or equal to 0");

                field.RuleFor(f => f.HolidaySurcharge)
                    .GreaterThanOrEqualTo(0).WithMessage("HolidaySurcharge must be greater than or equal to 0");

                field.RuleFor(f => f)
                    .Must(f => f.PeakStartTime.HasValue == f.PeakEndTime.HasValue)
                    .WithMessage("PeakStartTime and PeakEndTime must either both be provided or both be empty");

                field.RuleFor(f => f)
                    .Must(f => !f.PeakStartTime.HasValue || !f.PeakEndTime.HasValue || f.PeakStartTime.Value < f.PeakEndTime.Value)
                    .WithMessage("PeakStartTime must be earlier than PeakEndTime");
            });

            RuleFor(x => x.OperatingHours)
                .NotEmpty().WithMessage("At least one operating hour is required");

            RuleForEach(x => x.OperatingHours).ChildRules(oh =>
            {
                oh.RuleFor(o => o.OpenTime)
                    .LessThan(o => o.CloseTime).WithMessage("OpenTime must be before CloseTime");
            });
        }
    }
}
