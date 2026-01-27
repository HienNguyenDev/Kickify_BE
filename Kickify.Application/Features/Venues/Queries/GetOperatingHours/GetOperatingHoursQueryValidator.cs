using FluentValidation;

namespace Kickify.Application.Features.Venues.Queries.GetOperatingHours;

public class GetOperatingHoursQueryValidator : AbstractValidator<GetOperatingHoursQuery>
{
    public GetOperatingHoursQueryValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty()
            .WithMessage("VenueId is required");
    }
}
