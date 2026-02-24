using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public record CheckAvailabilityQuery(
        Guid FieldId,
        DateTime Date
    ) : IQuery<CheckAvailabilityResponse>;
}
