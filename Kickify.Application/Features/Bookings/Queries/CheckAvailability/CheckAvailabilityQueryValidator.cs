using FluentValidation;

namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public class CheckAvailabilityQueryValidator : AbstractValidator<CheckAvailabilityQuery>
    {
        public CheckAvailabilityQueryValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("FieldId is required");

            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Date must be today or in the future");
        }
    }
}
