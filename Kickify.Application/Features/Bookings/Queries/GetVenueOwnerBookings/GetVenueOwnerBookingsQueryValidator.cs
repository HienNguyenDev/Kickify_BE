using FluentValidation;

namespace Kickify.Application.Features.Bookings.Queries.GetVenueOwnerBookings;

public class GetVenueOwnerBookingsQueryValidator : AbstractValidator<GetVenueOwnerBookingsQuery>
{
    public GetVenueOwnerBookingsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
