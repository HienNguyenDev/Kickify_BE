using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Venues.Commands.AddField;
using Kickify.Application.Features.Venues.Commands.CreateVenue;
using Kickify.Application.Features.Venues.Queries.GetAllVenues;
using Kickify.Application.Features.Venues.Queries.GetVenueById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly ISender _sender;

        public VenuesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Create a new venue with fields and operating hours
        /// </summary>
        [HttpPost]
        public async Task<IResult> CreateVenue([FromBody] CreateVenueRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateVenueCommand(
                request.OwnerId,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.Description,
                request.Fields.Select(f => new CreateVenueFieldDto(
                    f.Name,
                    f.FieldType,
                    f.MaxPlayers,
                    f.PricePerHour,
                    f.Description
                )).ToList(),
                request.OperatingHours.Select(oh => new CreateVenueOperatingHoursDto(
                    oh.DayOfWeek,
                    oh.OpenTime,
                    oh.CloseTime
                )).ToList()
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get venue by ID with all details (fields, photos, operating hours)
        /// </summary>
        [HttpGet("{venueId:guid}")]
        public async Task<IResult> GetVenueById(Guid venueId, CancellationToken cancellationToken)
        {
            var query = new GetVenueByIdQuery(venueId);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Search venues with filters
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetAllVenues(
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] double? radiusKm,
            [FromQuery] DateTime? date,
            [FromQuery] string? sportType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllVenuesQuery(
                latitude,
                longitude,
                radiusKm,
                date,
                sportType,
                page,
                pageSize
            );

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Add a new field to existing venue
        /// </summary>
        [HttpPost("{venueId:guid}/fields")]
        public async Task<IResult> AddField(
            Guid venueId,
            [FromBody] AddFieldRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AddFieldCommand(
                venueId,
                request.Name,
                request.FieldType,
                request.MaxPlayers,
                request.PricePerHour,
                request.Description
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
