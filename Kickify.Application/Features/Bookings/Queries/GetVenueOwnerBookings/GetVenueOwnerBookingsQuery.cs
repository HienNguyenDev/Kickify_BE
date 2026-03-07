using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.GetVenueOwnerBookings;

public record GetVenueOwnerBookingsQuery(
    Guid? FieldId = null,
    DateTime? Date = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetVenueOwnerBookingsResponse>;
