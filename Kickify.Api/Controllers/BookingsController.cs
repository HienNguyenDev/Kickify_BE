using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Bookings.Commands.ProcessPayment;
using Kickify.Application.Features.Bookings.Queries.CheckAvailability;
using Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots;
using Kickify.Application.Features.Bookings.Queries.GetAllBookings;
using Kickify.Application.Features.Bookings.Queries.GetBookingById;
using Kickify.Application.Features.Bookings.Queries.GetBookingPreview;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly ISender _sender;

        public BookingsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Get all bookings with pagination and optional filters
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetAllBookings(
            [FromQuery] Guid? fieldId,
            [FromQuery] DateTime? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllBookingsQuery(fieldId, date, page, pageSize);
            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get booking by ID with full details
        /// </summary>
        [HttpGet("{bookingId:guid}")]
        public async Task<IResult> GetBookingById(Guid bookingId, CancellationToken cancellationToken)
        {
            var query = new GetBookingByIdQuery(bookingId);
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
            [FromQuery] int durationMinutes,
            [FromQuery] int numberOfPlayers,
            CancellationToken cancellationToken)
        {
            var query = new GetBookingPreviewQuery(
                fieldId,
                date,
                startTime,
                durationMinutes,
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
