using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public class GetVenueByIdQueryHandler : IQueryHandler<GetVenueByIdQuery, GetVenueByIdResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IWalletRepository _walletRepository;

        public GetVenueByIdQueryHandler(
            IVenueRepository venueRepository,
            IWalletRepository walletRepository)
        {
            _venueRepository = venueRepository;
            _walletRepository = walletRepository;
        }

        public async Task<Result<GetVenueByIdResponse>> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
        {
            var venue = await _venueRepository.GetVenueWithDetailsAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<GetVenueByIdResponse>(VenueErrors.NotFound(request.VenueId));
            }

            var wallet = await _walletRepository.GetByUserIdAsync(venue.OwnerId, cancellationToken);

            var response = new GetVenueByIdResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude ?? 0,
                venue.Longitude ?? 0,
                venue.ContactPhone,
                venue.ContactEmail,
                venue.Description,
                venue.Amenities,
                venue.Status.ToString(),
                venue.AdminNotes,
                venue.AverageRating,
                venue.TotalReviews,
                venue.Fields.Select(f => new VenueFieldDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHourSurcharge,
                    f.IsActive,
                    f.CreatedAt,
                    f.UpdatedAt
                )).ToList(),
                venue.VenueOperatingHours.Select(oh => new OperatingHoursDto(
                    (DayOfWeek)oh.DayOfWeek,
                    oh.OpenTime ?? TimeSpan.Zero,
                    oh.CloseTime ?? TimeSpan.Zero
                )).OrderBy(oh => oh.DayOfWeek).ToList(),
                venue.VenuePhotos.Select(p => new VenuePhotoDto(
                    p.PhotoId,
                    p.PhotoUrl,
                    p.DisplayOrder == 0
                )).ToList(),
                wallet?.Balance ?? 0,
                venue.CreatedAt,
                venue.UpdatedAt
            );

            return Result.Success(response);
        }
    }
}
