using FluentValidation;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdQueryValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty()
                .WithMessage("BookingId is required");
        }
    }
}
