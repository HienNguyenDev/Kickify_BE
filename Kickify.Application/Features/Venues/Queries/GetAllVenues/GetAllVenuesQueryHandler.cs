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

            var (venues, total) = await _venueRepository.SearchVenuesAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusKm,
                request.Date,
                fieldType,
                request.SearchName,
                request.Page,
                request.PageSize,
                cancellationToken
            );

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
                v.Fields.Select(f => new FieldSummaryDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHourSurcharge
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
