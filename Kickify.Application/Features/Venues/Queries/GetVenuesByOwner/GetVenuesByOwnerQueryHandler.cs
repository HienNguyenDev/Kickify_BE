using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public class GetVenuesByOwnerQueryHandler : IQueryHandler<GetVenuesByOwnerQuery, GetVenuesByOwnerResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserContext _userContext;

        public GetVenuesByOwnerQueryHandler(
            IVenueRepository venueRepository,
            IWalletRepository walletRepository,
            IUserContext userContext)
        {
            _venueRepository = venueRepository;
            _walletRepository = walletRepository;
            _userContext = userContext;
        }

        public async Task<Result<GetVenuesByOwnerResponse>> Handle(GetVenuesByOwnerQuery request, CancellationToken cancellationToken)
        {
            var ownerId = _userContext.UserId;

            var wallet = await _walletRepository.GetByUserIdAsync(ownerId, cancellationToken);
            var walletBalance = wallet?.Balance ?? 0;

            var (venues, total) = await _venueRepository.GetVenuesByOwnerPagedAsync(
                ownerId,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var venueItems = venues.Select(v => new OwnerVenueItemDto(
                v.VenueId,
                v.VenueName,
                v.Address,
                v.Latitude,
                v.Longitude,
                v.ContactPhone,
                v.ContactEmail,
                v.Description,
                v.Amenities,
                v.Status.ToString(),
                v.AverageRating,
                v.TotalReviews,
                v.Fields?.Select(f => new OwnerVenueFieldDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHourSurcharge,
                    f.IsActive,
                    f.CreatedAt,
                    f.UpdatedAt
                )).ToList() ?? new List<OwnerVenueFieldDto>(),
                v.VenueOperatingHours?.OrderBy(oh => oh.DayOfWeek).Select(oh => new OwnerVenueOperatingHoursDto(
                    oh.DayOfWeek.ToString(),
                    oh.OpenTime,
                    oh.CloseTime
                )).ToList() ?? new List<OwnerVenueOperatingHoursDto>(),
                v.VenuePhotos?.OrderBy(p => p.DisplayOrder).Select(p => new OwnerVenuePhotoDto(
                    p.PhotoId,
                    p.PhotoUrl,
                    p.DisplayOrder
                )).ToList() ?? new List<OwnerVenuePhotoDto>(),
                walletBalance,
                v.CreatedAt,
                v.UpdatedAt
            )).ToList();

            var response = new GetVenuesByOwnerResponse(
                venueItems,
                total,
                request.Page,
                request.PageSize
            );

            return Result.Success(response);
        }
    }
}
