using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public record GetVenueByIdQuery(Guid VenueId) : IRequest<Result<GetVenueByIdResponse>>;
}
