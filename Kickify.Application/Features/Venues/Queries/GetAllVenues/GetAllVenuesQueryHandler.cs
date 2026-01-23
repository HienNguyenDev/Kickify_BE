using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public class GetAllVenuesQueryHandler : IRequestHandler<GetAllVenuesQuery, Result<GetAllVenuesResponse>>
    {
        private readonly IVenueRepository _venueRepository;

        public GetAllVenuesQueryHandler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<Result<GetAllVenuesResponse>> Handle(GetAllVenuesQuery request, CancellationToken cancellationToken)
        {
            FieldType? sportType = null;
            if (!string.IsNullOrEmpty(request.SportType))
            {
                if (Enum.TryParse<FieldType>(request.SportType, true, out var parsed))
                {
                    sportType = parsed;
                }
            }

            var (venues, total) = await _venueRepository.SearchVenuesAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusKm,
                request.Date,
                sportType,
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
                v.Description,
                v.Fields.Select(f => new FieldSummaryDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.HourlyRate
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
