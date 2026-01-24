using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public record UpdateVenueCommand(
        Guid VenueId,
        Guid UserId,
        string? Name,
        string? Address,
        decimal? Latitude,
        decimal? Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities
    ) : IRequest<Result<UpdateVenueResponse>>;
}
