using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.VenuePhotos.Commands.DeleteVenuePhoto
{
    public record DeleteVenuePhotoCommand(
        Guid PhotoId,
        Guid UserId
    ) : IRequest<Result<DeleteVenuePhotoResponse>>;
}
