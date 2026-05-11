using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public class GetAllVenuesQueryHandler : IQueryHandler<GetAllVenuesQuery, GetAllVenuesResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public GetAllVenuesQueryHandler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<Result<GetAllVenuesResponse>> Handle(GetAllVenuesQuery request, CancellationToken cancellationToken)
        {
            FieldType? fieldType = null;
            if (!string.IsNullOrEmpty(request.FieldType))
            {
                if (Enum.TryParse<FieldType>(request.FieldType, true, out var parsed))
                {
                    fieldType = parsed;
                }
            }

            VenueStatus? venueStatus = null;
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<VenueStatus>(request.Status, true, out var parsedStatus))
                {
                    venueStatus = parsedStatus;
                }
            }

            // When no status filter is provided, exclude Archived venues from public listing
            bool excludeArchived = !venueStatus.HasValue;

            var (venues, total) = await _venueRepository.SearchVenuesAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusKm,
                request.Date,
                fieldType,
                request.SearchName,
                venueStatus,
                request.Page,
                request.PageSize,
                excludeArchived,
                cancellationToken
            );

            var venueIds = venues.Select(v => v.VenueId).ToList();
            var bookingCounts = await _venueRepository.GetBookingCountsByVenueIdsAsync(venueIds, cancellationToken);

            var venueItems = venues.Select(v => new VenueItemDto(
                v.VenueId,
                v.VenueName,
                v.Address,
                v.Latitude ?? 0,
                v.Longitude ?? 0,
                v.ContactPhone,
                v.ContactEmail,
                v.Description,
                v.Amenities,
                v.Status.ToString(),
                v.AdminNotes,
                v.AverageRating,
                v.VenueReviews.Count,
                bookingCounts.GetValueOrDefault(v.VenueId, 0),
                new VenueOwnerDto(
                    v.Owner.UserId,
                    v.Owner.FullName,
                    v.Owner.Phone,
                    v.Owner.AvatarUrl,
                    v.Owner.Bio,
                    v.Owner.DateOfBirth,
                    v.Owner.Gender?.ToString(),
                    v.Owner.Role.ToString(),
                    v.Owner.PreferredPositions,
                    v.Owner.ShirtNumber,
                    v.Owner.PreferredFoot,
                    v.Owner.IsActive
                ),
                v.Fields.Select(f => new FieldSummaryDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHours.Select(ph => new FieldPeakHourResponseDto(
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
                )).ToList(),
                v.VenuePhotos.FirstOrDefault(p => p.DisplayOrder == 0)?.PhotoUrl,
                v.CreatedAt
            )).ToList();

            var response = new GetAllVenuesResponse(
                venueItems,
                total,
                request.Page,
                request.PageSize,
                (int)Math.Ceiling(total / (double)request.PageSize)
            );

            return Result.Success(response);
        }
    }
}
