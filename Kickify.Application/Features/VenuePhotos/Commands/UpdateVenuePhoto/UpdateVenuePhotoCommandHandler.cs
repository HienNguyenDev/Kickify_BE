using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenuePhotos.Commands.UpdateVenuePhoto
{
    public class UpdateVenuePhotoCommandHandler : IRequestHandler<UpdateVenuePhotoCommand, Result<UpdateVenuePhotoResponse>>
    {
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateVenuePhotoCommandHandler> _logger;

        public UpdateVenuePhotoCommandHandler(
            IVenuePhotoRepository venuePhotoRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateVenuePhotoCommandHandler> logger)
        {
            _venuePhotoRepository = venuePhotoRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<UpdateVenuePhotoResponse>> Handle(UpdateVenuePhotoCommand request, CancellationToken cancellationToken)
        {
            // Get photo with venue info for ownership check TEST
            var photo = await _venuePhotoRepository.GetPhotoWithVenueAsync(request.PhotoId, cancellationToken);
            if (photo == null)
            {
                return Result.Failure<UpdateVenuePhotoResponse>(VenuePhotoErrors.NotFound(request.PhotoId));
            }

            // Check if user is the owner
            if (photo.Venue.OwnerId != request.UserId)
            {
                return Result.Failure<UpdateVenuePhotoResponse>(VenuePhotoErrors.Unauthorized);
            }

            // Update display order if provided
            if (request.DisplayOrder.HasValue)
            {
                photo.DisplayOrder = request.DisplayOrder.Value;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated photo {PhotoId} for venue {VenueId}", 
                photo.PhotoId, photo.VenueId);

            return Result.Success(new UpdateVenuePhotoResponse(
                photo.PhotoId,
                photo.VenueId,
                photo.PhotoUrl,
                photo.DisplayOrder
            ));
        }
    }
}
