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

            RuleFor(x => x.PeakHourSurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("PeakHourSurcharge must be greater than or equal to 0");

            RuleFor(x => x.WeekendSurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("WeekendSurcharge must be greater than or equal to 0");

            RuleFor(x => x.HolidaySurcharge)
                .GreaterThanOrEqualTo(0).WithMessage("HolidaySurcharge must be greater than or equal to 0");

            RuleFor(x => x)
                .Must(x => x.PeakStartTime.HasValue == x.PeakEndTime.HasValue)
                .WithMessage("PeakStartTime and PeakEndTime must either both be provided or both be empty");

            RuleFor(x => x)
                .Must(x => !x.PeakStartTime.HasValue || !x.PeakEndTime.HasValue || x.PeakStartTime.Value < x.PeakEndTime.Value)
                .WithMessage("PeakStartTime must be earlier than PeakEndTime");

            
        }
    }
}
