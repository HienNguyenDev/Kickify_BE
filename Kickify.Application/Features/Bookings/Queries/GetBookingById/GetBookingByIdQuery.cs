using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid BookingId) : IRequest<Result<GetBookingByIdResponse>>;
}
