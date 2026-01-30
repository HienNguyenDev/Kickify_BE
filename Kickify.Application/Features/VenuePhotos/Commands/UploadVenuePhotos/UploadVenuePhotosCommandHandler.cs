using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenuePhotos.Commands.UploadVenuePhotos
{
    public class UploadVenuePhotosCommandHandler : ICommandHandler<UploadVenuePhotosCommand, UploadVenuePhotosResponse>
    {
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UploadVenuePhotosCommandHandler> _logger;
        private readonly IUserContext _userContext;

        public UploadVenuePhotosCommandHandler(
            IVenuePhotoRepository venuePhotoRepository,
            IStorageService storageService,
            IUnitOfWork unitOfWork,
            ILogger<UploadVenuePhotosCommandHandler> logger,
            IUserContext userContext)
        {
            _venuePhotoRepository = venuePhotoRepository;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<Result<UploadVenuePhotosResponse>> Handle(UploadVenuePhotosCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Check if venue exists
            var venueExists = await _venuePhotoRepository.VenueExistsAsync(request.VenueId, cancellationToken);
            if (!venueExists)
            {
                return Result.Failure<UploadVenuePhotosResponse>(VenuePhotoErrors.VenueNotFound);
            }

            // Check if user is the owner
            var isOwner = await _venuePhotoRepository.IsVenueOwnerAsync(request.VenueId, userId, cancellationToken);
            if (!isOwner)
            {
                return Result.Failure<UploadVenuePhotosResponse>(VenuePhotoErrors.Unauthorized);
            }

            if (request.Photos.Count == 0)
            {
                return Result.Failure<UploadVenuePhotosResponse>(VenuePhotoErrors.NoPhotosProvided);
            }

            try
            {
                // Upload photos
                var uploadResults = await _storageService.UploadMultipleAsync(request.Photos, cancellationToken);

                var failedUploads = uploadResults.Where(r => !r.Success).ToList();
                if (failedUploads.Count > 0)
                {
                    // Rollback successful uploads
                    var successfulUploads = uploadResults.Where(r => r.Success).ToList();
                    foreach (var upload in successfulUploads)
                    {
                        await _storageService.DeleteAsync(upload.ObjectName, cancellationToken);
                    }

                    var errors = string.Join(", ", failedUploads.Select(f => f.ErrorMessage));
                    return Result.Failure<UploadVenuePhotosResponse>(VenuePhotoErrors.UploadFailed(errors));
                }

                // Get current max display order from database via repository
                var maxDisplayOrder = await _venuePhotoRepository.GetMaxDisplayOrderAsync(request.VenueId, cancellationToken);
                maxDisplayOrder++;

                var photoList = new List<VenuePhoto>();
                foreach (var upload in uploadResults)
                {
                    var photo = new VenuePhoto
                    {
                        PhotoId = Guid.NewGuid(),
                        VenueId = request.VenueId,
                        PhotoUrl = upload.PublicUrl,
                        DisplayOrder = maxDisplayOrder++,
                        CreatedAt = DateTime.UtcNow
                    };
                    photoList.Add(photo);
                }

                // Add photos via repository
                await _venuePhotoRepository.AddPhotosAsync(photoList, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Uploaded {PhotoCount} photos for venue {VenueId}", 
                    photoList.Count, request.VenueId);

                return Result.Success(new UploadVenuePhotosResponse(
                    request.VenueId,
                    photoList.Select(p => new VenuePhotoDto(
                        p.PhotoId,
                        p.PhotoUrl,
                        p.DisplayOrder
                    )).ToList()
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for venue {VenueId}", request.VenueId);
                throw;
            }
        }
    }
}
