using FluentValidation;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingPreview
{
    public class GetBookingPreviewQueryValidator : AbstractValidator<GetBookingPreviewQuery>
    {
        public GetBookingPreviewQueryValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("FieldId is required");

            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Date must be today or in the future");

            RuleFor(x => x.DurationMinutes)
                .Must(d => d == 60 || d == 90 || d == 120)
                .WithMessage("Duration must be 60, 90, or 120 minutes");

            RuleFor(x => x.NumberOfPlayers)
                .GreaterThan(0).WithMessage("NumberOfPlayers must be greater than 0");
        }
    }
}
