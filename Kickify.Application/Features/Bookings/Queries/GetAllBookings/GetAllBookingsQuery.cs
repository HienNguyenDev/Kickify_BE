using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.GetAllBookings
{
    public record GetAllBookingsQuery(
        Guid? FieldId = null,
        DateTime? Date = null,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetAllBookingsResponse>>;
}
