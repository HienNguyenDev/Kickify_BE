using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public class GetFieldsByVenueQueryHandler : IRequestHandler<GetFieldsByVenueQuery, Result<GetFieldsByVenueResponse>>
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
                f.PeakHourSurcharge,
                f.IsActive,
                f.CreatedAt
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
