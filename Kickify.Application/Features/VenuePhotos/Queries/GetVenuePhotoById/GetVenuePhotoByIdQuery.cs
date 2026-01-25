using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotoById
{
    public record GetVenuePhotoByIdQuery(
        Guid PhotoId
    ) : IRequest<Result<GetVenuePhotoByIdResponse>>;
}
