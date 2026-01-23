using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public record DeleteVenueCommand(
        Guid VenueId,
        Guid UserId
    ) : IRequest<Result<DeleteVenueResponse>>;
}
