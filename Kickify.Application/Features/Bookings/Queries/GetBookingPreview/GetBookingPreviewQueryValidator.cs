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

            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("StartTime must be before EndTime");

            RuleFor(x => x.NumberOfPlayers)
                .GreaterThan(0).WithMessage("NumberOfPlayers must be greater than 0");
        }
    }
}
