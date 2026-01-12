using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public record CheckAvailabilityQuery(
        Guid FieldId,
        DateTime Date
    ) : IRequest<Result<CheckAvailabilityResponse>>;
}
