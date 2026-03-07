using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Fields.Commands.BlockFieldSlot;
using Kickify.Application.Features.Venues.Commands.AddField;
using Kickify.Application.Features.Venues.Commands.CreateVenue;
using Kickify.Application.Features.Venues.Commands.DeleteVenue;
using Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;
using Kickify.Application.Features.Venues.Commands.UpdateVenue;
using Kickify.Application.Features.Venues.Commands.UpdateVenueStatus;
using Kickify.Application.Features.Venues.Commands.ToggleVenueSuspension;
using Kickify.Application.Features.Venues.Queries.GetAllVenues;
using Kickify.Application.Features.Venues.Queries.GetFieldsByVenue;
using Kickify.Application.Features.Venues.Queries.GetOperatingHours;
using Kickify.Application.Features.Venues.Queries.GetVenueById;
using Kickify.Application.Features.Venues.Queries.GetVenuesByOwner;
using Kickify.Application.Features.VenueReviews.Commands.CreateVenueReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/venues")]
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
        [Authorize]
        [HttpPost]
        public async Task<IResult> CreateVenue([FromBody] CreateVenueRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateVenueCommand(
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.ContactPhone,
                request.ContactEmail,
                request.Description,
                request.Amenities,
                request.Fields.Select(f => new CreateVenueFieldDto(
                    f.Name,
                    f.FieldType,
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHourSurcharge
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
        /// Search venues with filters.
        /// When fieldType is specified (e.g., SevenVsSeven), only venues with matching fields are returned,
        /// and only the matching fields are included in the response.
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetAllVenues(
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] double? radiusKm,
            [FromQuery] DateTime? date,
            [FromQuery] string? fieldType,
            [FromQuery] string? searchName,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllVenuesQuery(
                latitude,
                longitude,
                radiusKm,
                date,
                fieldType,
                searchName,
                status,
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
                request.SurfaceType,
                request.HourlyRate,
                request.PeakHourSurcharge
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get venues owned by the current user
        /// </summary>
        [Authorize]
        [HttpGet("mine")]
        public async Task<IResult> GetMyVenues(
            [FromQuery] string? searchName,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetVenuesByOwnerQuery(searchName, status, page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get all fields of a venue
        /// </summary>
        [HttpGet("{venueId:guid}/fields")]
        public async Task<IResult> GetFieldsByVenue(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFieldsByVenueQuery(venueId);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get operating hours of a venue (ordered by DayOfWeek)
        /// </summary>
        [HttpGet("{venueId:guid}/operating-hours")]
        public async Task<IResult> GetOperatingHours(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            var query = new GetOperatingHoursQuery { VenueId = venueId };
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update operating hours of a venue (owner only)
        /// Requires exactly 7 items (one for each day of the week).
        /// Logic: null = keep existing, "" = clear/close, valid time = update
        /// </summary>
        [Authorize]
        [HttpPut("{venueId:guid}/operating-hours")]
        public async Task<IResult> UpdateOperatingHours(
            Guid venueId,
            [FromBody] UpdateOperatingHoursRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateOperatingHoursCommand
            {
                VenueId = venueId,
                OperatingHours = request.OperatingHours.Select(oh => new OperatingHourItemDto(
                    oh.DayOfWeek,
                    oh.OpenTime,
                    oh.CloseTime
                )).ToList()
            };

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update venue (owner only)
        /// </summary>
        [Authorize]
        [HttpPut("{venueId:guid}")]
        public async Task<IResult> UpdateVenue(
            Guid venueId,
            [FromBody] UpdateVenueRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateVenueCommand(
                venueId,
                request.Name,
                request.Address,
                request.Latitude,
                request.Longitude,
                request.ContactPhone,
                request.ContactEmail,
                request.Description,
                request.Amenities,
                request.OperatingHours?.Select(oh => new UpdateVenueOperatingHourItemDto(
                    oh.DayOfWeek,
                    oh.OpenTime,
                    oh.CloseTime
                )).ToList()
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>       
        /// Delete venue (owner only)
        /// </summary>
        [Authorize]
        [HttpDelete("{venueId:guid}")]
        public async Task<IResult> DeleteVenue(Guid venueId, CancellationToken cancellationToken)
        {
            var command = new DeleteVenueCommand(venueId);
            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Block a time slot on a field (owner only)
        /// Used for offline bookings or field maintenance
        /// Creates a "ghost" booking that marks the slot as unavailable
        /// </summary>
        [Authorize]
        [HttpPost("{venueId:guid}/fields/{fieldId:guid}/block")]
        public async Task<IResult> BlockFieldSlot(
            Guid venueId,
            Guid fieldId,
            [FromBody] BlockFieldSlotRequest request,
            CancellationToken cancellationToken)
        {
            var command = new BlockFieldSlotCommand(
                venueId,
                fieldId,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.Reason,
                request.Amount
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update venue status (Admin only). Used to approve, reject, or suspend a venue.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{venueId:guid}/status")]
        public async Task<IResult> UpdateVenueStatus(
            Guid venueId,
            [FromBody] UpdateVenueStatusRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateVenueStatusCommand(
                venueId,
                request.Status,
                request.AdminNotes
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Toggle venue suspension (VenueOwner only).
        /// Approved → Suspended (close venue), Suspended → Approved (reopen venue).
        /// </summary>
        [Authorize(Roles = "VenueOwner")]
        [HttpPatch("{venueId:guid}/toggle-suspension")]
        public async Task<IResult> ToggleVenueSuspension(
            Guid venueId,
            CancellationToken cancellationToken)
        {
            var command = new ToggleVenueSuspensionCommand(venueId);
            var result = await _sender.Send(command, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Create a venue review. Only requires BookingId — VenueId is derived from Booking → Field → Venue.
        /// Validates: participation in match room, match has ended, booking/room completed, no duplicate review.
        /// </summary>
        [Authorize]
        [HttpPost("reviews")]
        public async Task<IResult> CreateVenueReview(
            [FromBody] CreateVenueReviewRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateVenueReviewCommand(
                request.BookingId,
                request.Rating,
                request.Comment
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
