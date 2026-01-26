using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenuePhotos.Commands.DeleteVenuePhoto
{
    public class DeleteVenuePhotoCommandHandler : ICommandHandler<DeleteVenuePhotoCommand, DeleteVenuePhotoResponse>
    {
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteVenuePhotoCommandHandler> _logger;

        public DeleteVenuePhotoCommandHandler(
            IVenuePhotoRepository venuePhotoRepository,
            IStorageService storageService,
            IUnitOfWork unitOfWork,
            ILogger<DeleteVenuePhotoCommandHandler> logger)
        {
            _venuePhotoRepository = venuePhotoRepository;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<DeleteVenuePhotoResponse>> Handle(DeleteVenuePhotoCommand request, CancellationToken cancellationToken)
        {
            // Get photo with venue info for ownership check
            var photo = await _venuePhotoRepository.GetPhotoWithVenueAsync(request.PhotoId, cancellationToken);
            if (photo == null)
            {
                return Result.Failure<DeleteVenuePhotoResponse>(VenuePhotoErrors.NotFound(request.PhotoId));
            }

            // Check if user is the owner
            if (photo.Venue.OwnerId != request.UserId)
            {
                return Result.Failure<DeleteVenuePhotoResponse>(VenuePhotoErrors.Unauthorized);
            }

            try
            {
                // Extract object name from URL for deletion from storage
                var uri = new Uri(photo.PhotoUrl);
                var objectName = uri.AbsolutePath.TrimStart('/');
                
                // Delete from storage
                await _storageService.DeleteAsync(objectName, cancellationToken);

                // Delete from database
                _venuePhotoRepository.Remove(photo);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted photo {PhotoId} for venue {VenueId}", 
                    request.PhotoId, photo.VenueId);

                return Result.Success(new DeleteVenuePhotoResponse(
                    request.PhotoId,
                    true,
                    "Photo deleted successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo {PhotoId}", request.PhotoId);
                return Result.Failure<DeleteVenuePhotoResponse>(VenuePhotoErrors.DeleteFailed);
            }
        }
    }
}
