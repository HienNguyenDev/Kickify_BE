using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.GetAllBookings
{
    public record GetAllBookingsQuery(
        Guid? FieldId = null,
        DateTime? Date = null,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetAllBookingsResponse>;
}
