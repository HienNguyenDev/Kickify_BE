using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

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

            VenueStatus? venueStatus = null;
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<VenueStatus>(request.Status, true, out var parsedStatus))
                {
                    venueStatus = parsedStatus;
                }
            }

            var (venues, total) = await _venueRepository.GetVenuesByOwnerPagedAsync(
                ownerId,
                request.SearchName,
                venueStatus,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var venueIds = venues.Select(v => v.VenueId).ToList();
            var bookingCounts = await _venueRepository.GetBookingCountsByVenueIdsAsync(venueIds, cancellationToken);

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
                v.AdminNotes,
                v.AverageRating,
                v.VenueReviews.Count,
                bookingCounts.GetValueOrDefault(v.VenueId, 0),
                v.Fields?.Select(f => new OwnerVenueFieldDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHours.Select(ph => new OwnerVenueFieldPeakHourResponseDto(
                        ph.Id,
                        ph.StartTime,
                        ph.EndTime,
                        ph.SurchargeAmount,
                        ph.IsPercentage,
                        ph.ApplicableDays
                    )).ToList(),
                    f.WeekendSurcharge,
                    f.HolidaySurcharge,
                    f.IsWeekendSurchargePercentage,
                    f.IsHolidaySurchargePercentage,
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
