using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Bookings.Queries.CheckAvailability;
using Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots;
using Kickify.Application.Features.Fields.Commands.DeleteField;
using Kickify.Application.Features.Fields.Commands.UpdateField;
using Kickify.Application.Features.Fields.Queries.GetAllFields;
using Kickify.Application.Features.Fields.Queries.GetFieldById;
using Kickify.Application.Features.Fields.Queries.GetFieldsByOwner;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/fields")]
    public class FieldsController : ControllerBase
    {
        private readonly ISender _sender;

        public FieldsController(ISender sender)
        {
            _sender = sender;
        }


        /// <summary>
        /// Check availability for a field on a specific date (returns 30-minute slots)
        /// </summary>
        [HttpGet("{fieldId:guid}/availability")]
        public async Task<IResult> CheckAvailability(
            [FromRoute] Guid fieldId,
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
        {
            var query = new CheckAvailabilityQuery(fieldId, date);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Check if consecutive time slots are available for a specific duration
        /// Used when host selects start time + duration (60/90/120 minutes)
        /// </summary>
        [HttpGet("{fieldId:guid}/check-consecutive-slots")]
        public async Task<IResult> CheckConsecutiveSlots(
            [FromRoute] Guid fieldId,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan startTime,
            [FromQuery] int durationMinutes,
            CancellationToken cancellationToken)
        {
            var query = new CheckConsecutiveSlotsQuery(
                fieldId,
                date,
                startTime,
                durationMinutes
            );

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get all fields with pagination and optional filters
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetAllFields(
            [FromQuery] string? fieldType,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllFieldsQuery(fieldType, isActive, page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get field by ID with venue and operating hours
        /// </summary>
        [HttpGet("{fieldId:guid}")]
        public async Task<IResult> GetFieldById(Guid fieldId, CancellationToken cancellationToken)
        {
            var query = new GetFieldByIdQuery(fieldId);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get fields by owner ID (all fields across all venues owned by the user)
        /// </summary>
        [HttpGet("owner/{ownerId:guid}")]
        public async Task<IResult> GetFieldsByOwner(
            Guid ownerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFieldsByOwnerQuery(ownerId, page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update field (owner only)
        /// </summary>
        [Authorize]
        [HttpPut("{fieldId:guid}")]
        public async Task<IResult> UpdateField(
            Guid fieldId,
            [FromBody] UpdateFieldRequest request,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateFieldCommand(
                fieldId,
                userId,
                request.Name,
                request.FieldType,
                request.SurfaceType,
                request.HourlyRate,
                request.PeakHourSurcharge,
                request.IsActive
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Delete field (owner only)
        /// </summary>
        [Authorize]
        [HttpDelete("{fieldId:guid}")]
        public async Task<IResult> DeleteField(Guid fieldId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new DeleteFieldCommand(fieldId, userId);
            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
