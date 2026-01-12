using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Bookings.Commands.ProcessPayment;
using Kickify.Application.Features.Bookings.Queries.CheckAvailability;
using Kickify.Application.Features.Bookings.Queries.GetBookingPreview;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ISender _sender;

        public BookingsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Check availability for a field on a specific date
        /// </summary>
        [HttpGet("availability")]
        public async Task<IResult> CheckAvailability(
            [FromQuery] Guid fieldId,
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
        {
            var query = new CheckAvailabilityQuery(fieldId, date);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get booking preview with pricing calculation
        /// </summary>
        [HttpGet("preview")]
        public async Task<IResult> GetBookingPreview(
            [FromQuery] Guid fieldId,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan startTime,
            [FromQuery] TimeSpan endTime,
            [FromQuery] int numberOfPlayers,
            CancellationToken cancellationToken)
        {
            var query = new GetBookingPreviewQuery(
                fieldId,
                date,
                startTime,
                endTime,
                numberOfPlayers
            );

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Process payment for a room participant (with race condition handling)
        /// </summary>
        [HttpPost("process-payment")]
        public async Task<IResult> ProcessPayment(
            [FromBody] ProcessPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ProcessPaymentCommand(
                request.RoomId,
                request.UserId
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
