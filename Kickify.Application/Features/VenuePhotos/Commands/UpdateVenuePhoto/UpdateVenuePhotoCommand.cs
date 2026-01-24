using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto
{
    public record UpdateVenuePhotoCommand(
        Guid PhotoId,
        Guid UserId,
        int? DisplayOrder
    ) : IRequest<Result<UpdateVenuePhotoResponse>>;
}
