using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.VenuePhotos.Queries.GetVenuePhotoById
{
    public class GetVenuePhotoByIdQueryHandler : IQueryHandler<GetVenuePhotoByIdQuery, GetVenuePhotoByIdResponse>
    {
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly ILogger<GetVenuePhotoByIdQueryHandler> _logger;

        public GetVenuePhotoByIdQueryHandler(
            IVenuePhotoRepository venuePhotoRepository,
            ILogger<GetVenuePhotoByIdQueryHandler> logger)
        {
            _venuePhotoRepository = venuePhotoRepository;
            _logger = logger;
        }

        public async Task<Result<GetVenuePhotoByIdResponse>> Handle(GetVenuePhotoByIdQuery request, CancellationToken cancellationToken)
        {
            var photo = await _venuePhotoRepository.GetByIdAsync(request.PhotoId);
            if (photo == null)
            {
                return Result.Failure<GetVenuePhotoByIdResponse>(VenuePhotoErrors.NotFound(request.PhotoId));
            }

            _logger.LogInformation("Retrieved photo {PhotoId}", request.PhotoId);

            return Result.Success(new GetVenuePhotoByIdResponse(
                photo.PhotoId,
                photo.VenueId,
                photo.PhotoUrl,
                photo.DisplayOrder,
                photo.CreatedAt
            ));
        }
    }
}
