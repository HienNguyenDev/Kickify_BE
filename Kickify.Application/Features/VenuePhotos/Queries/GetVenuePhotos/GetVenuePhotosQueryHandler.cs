using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotos
{
    public class GetVenuePhotosQueryHandler : IRequestHandler<GetVenuePhotosQuery, Result<GetVenuePhotosResponse>>
    {
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly ILogger<GetVenuePhotosQueryHandler> _logger;

        public GetVenuePhotosQueryHandler(
            IVenuePhotoRepository venuePhotoRepository,
            ILogger<GetVenuePhotosQueryHandler> logger)
        {
            _venuePhotoRepository = venuePhotoRepository;
            _logger = logger;
        }

        public async Task<Result<GetVenuePhotosResponse>> Handle(GetVenuePhotosQuery request, CancellationToken cancellationToken)
        {
            // Check if venue exists
            var venueExists = await _venuePhotoRepository.VenueExistsAsync(request.VenueId, cancellationToken);
            if (!venueExists)
            {
                return Result.Failure<GetVenuePhotosResponse>(VenuePhotoErrors.VenueNotFound);
            }

            // Get all photos
            var photos = await _venuePhotoRepository.GetPhotosByVenueIdAsync(request.VenueId, cancellationToken);

            _logger.LogInformation("Retrieved {PhotoCount} photos for venue {VenueId}", 
                photos.Count(), request.VenueId);

            return Result.Success(new GetVenuePhotosResponse(
                request.VenueId,
                photos.Select(p => new VenuePhotoItemDto(
                    p.PhotoId,
                    p.PhotoUrl,
                    p.DisplayOrder,
                    p.CreatedAt
                )).ToList()
            ));
        }
    }
}
