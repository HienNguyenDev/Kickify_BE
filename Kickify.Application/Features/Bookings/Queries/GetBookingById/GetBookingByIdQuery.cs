using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid BookingId) : IQuery<GetBookingByIdResponse>;
}
