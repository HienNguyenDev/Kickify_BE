using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, GetBookingByIdResponse>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<GetBookingByIdResponse>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(request.BookingId, cancellationToken);

            if (booking == null)
            {
                return Result.Failure<GetBookingByIdResponse>(BookingErrors.NotFound(request.BookingId));
            }

            var response = new GetBookingByIdResponse(
                booking.BookingId,
                booking.RoomId,
                booking.FieldId,
                booking.Field?.FieldName ?? string.Empty,
                booking.Field?.VenueId ?? Guid.Empty,
                booking.Field?.Venue?.VenueName ?? string.Empty,
                booking.Field?.Venue?.Address ?? string.Empty,
                booking.BookingDate,
                booking.StartTime,
                booking.EndTime,
                booking.TotalAmount,
                booking.PlatformFee,
                booking.VenueAmount,
                booking.Status.ToString(),
                booking.PaymentMethod,
                booking.TransactionReference,
                booking.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
