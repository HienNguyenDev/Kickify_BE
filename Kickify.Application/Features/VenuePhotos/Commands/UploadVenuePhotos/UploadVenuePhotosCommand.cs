using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.VenuePhotos.Commands.UploadVenuePhotos
{
    public record UploadVenuePhotosCommand(
        Guid VenueId,
        Guid UserId,
        List<FileUploadRequest> Photos
    ) : IRequest<Result<UploadVenuePhotosResponse>>;
}
