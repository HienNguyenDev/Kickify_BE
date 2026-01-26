using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public class GetVenueByIdQueryHandler : IQueryHandler<GetVenueByIdQuery, GetVenueByIdResponse>
    {
        private readonly IVenueRepository _venueRepository;

        public GetVenueByIdQueryHandler(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<Result<GetVenueByIdResponse>> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
        {
            var venue = await _venueRepository.GetVenueWithDetailsAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<GetVenueByIdResponse>(VenueErrors.NotFound(request.VenueId));
            }

            var response = new GetVenueByIdResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude ?? 0,
                venue.Longitude ?? 0,
                venue.Description,
                venue.Fields.Select(f => new VenueFieldDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    0, // MaxPlayers not in entity
                    f.HourlyRate,
                    null // Description not in entity
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
                venue.VenueWallet?.Balance ?? 0,
                venue.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
