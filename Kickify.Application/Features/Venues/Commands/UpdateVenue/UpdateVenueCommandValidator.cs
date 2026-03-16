using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public class UpdateVenueCommandValidator : AbstractValidator<UpdateVenueCommand>
    {
        public UpdateVenueCommandValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty()
                .WithMessage("VenueId is required");

            When(x => !string.IsNullOrEmpty(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MaximumLength(200)
                    .WithMessage("Name must not exceed 200 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Address), () =>
            {
                RuleFor(x => x.Address)
                    .MaximumLength(500)
                    .WithMessage("Address must not exceed 500 characters");
            });

            When(x => x.Latitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude)
                    .InclusiveBetween(-90, 90)
                    .WithMessage("Latitude must be between -90 and 90");
            });

            When(x => x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Longitude)
                    .InclusiveBetween(-180, 180)
                    .WithMessage("Longitude must be between -180 and 180");
            });

            When(x => !string.IsNullOrEmpty(x.ContactPhone), () =>
            {
                RuleFor(x => x.ContactPhone)
                    .MaximumLength(20)
                    .WithMessage("ContactPhone must not exceed 20 characters");
            });

            When(x => !string.IsNullOrEmpty(x.ContactEmail), () =>
            {
                RuleFor(x => x.ContactEmail)
                    .EmailAddress()
                    .WithMessage("ContactEmail must be a valid email address");
            });

            When(x => x.IgnoredHolidayIds != null, () =>
            {
                RuleFor(x => x.IgnoredHolidayIds!)
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("IgnoredHolidayIds must not contain duplicate values");
            });
        }
    }
}
