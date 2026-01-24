using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotos
{
    public record GetVenuePhotosQuery(
        Guid VenueId
    ) : IRequest<Result<GetVenuePhotosResponse>>;
}
