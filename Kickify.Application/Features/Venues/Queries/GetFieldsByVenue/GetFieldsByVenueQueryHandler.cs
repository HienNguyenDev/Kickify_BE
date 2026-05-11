using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public class GetFieldsByVenueQueryHandler : IQueryHandler<GetFieldsByVenueQuery, GetFieldsByVenueResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IFieldRepository _fieldRepository;

        public GetFieldsByVenueQueryHandler(
            IVenueRepository venueRepository,
            IFieldRepository fieldRepository)
        {
            _venueRepository = venueRepository;
            _fieldRepository = fieldRepository;
        }

        public async Task<Result<GetFieldsByVenueResponse>> Handle(GetFieldsByVenueQuery request, CancellationToken cancellationToken)
        {
            var venue = await _venueRepository.GetByIdAsync(request.VenueId);

            if (venue == null)
            {
                return Result.Failure<GetFieldsByVenueResponse>(VenueErrors.NotFound(request.VenueId));
            }

            var fields = await _fieldRepository.GetFieldsByVenueAsync(request.VenueId, cancellationToken);

            var fieldItems = fields.Select(f => new VenueFieldItemDto(
                f.FieldId,
                f.FieldName,
                f.FieldType.ToString(),
                f.SurfaceType,
                f.HourlyRate,
                f.PeakHours.Select(ph => new VenueFieldPeakHourResponseDto(
                    ph.Id,
                    ph.StartTime,
                    ph.EndTime,
                    ph.SurchargeAmount,
                    ph.IsPercentage,
                    ph.ApplicableDays
                )).ToList(),
                f.WeekendSurcharge,
                f.HolidaySurcharge,
                f.IsActive,
                f.CreatedAt,
                f.IsWeekendSurchargePercentage,
                f.IsHolidaySurchargePercentage
            )).ToList();

            var response = new GetFieldsByVenueResponse(
                venue.VenueId,
                venue.VenueName,
                fieldItems
            );

            return Result.Success(response);
        }
    }
}
