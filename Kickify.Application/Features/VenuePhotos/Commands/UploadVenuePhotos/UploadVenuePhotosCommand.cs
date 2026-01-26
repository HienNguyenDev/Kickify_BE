using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Application.Features.VenuePhotos.Commands.UploadVenuePhotos
{
    public record UploadVenuePhotosCommand(
        Guid VenueId,
        Guid UserId,
        List<FileUploadRequest> Photos
    ) : ICommand<UploadVenuePhotosResponse>;
}
