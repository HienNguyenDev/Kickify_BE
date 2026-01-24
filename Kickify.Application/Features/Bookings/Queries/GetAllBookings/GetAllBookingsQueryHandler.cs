using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.GetAllBookings
{
    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, Result<GetAllBookingsResponse>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetAllBookingsQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<GetAllBookingsResponse>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
        {
            var (bookings, total) = await _bookingRepository.GetBookingsPagedAsync(
                request.FieldId,
                request.Date,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var bookingItems = bookings.Select(b => new BookingItemDto(
                b.BookingId,
                b.RoomId,
                b.FieldId,
                b.Field?.FieldName ?? string.Empty,
                b.Field?.Venue?.VenueName ?? string.Empty,
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.TotalAmount,
                b.Status.ToString(),
                b.CreatedAt
            )).ToList();

            var response = new GetAllBookingsResponse(
                bookingItems,
                total,
                request.Page,
                request.PageSize,
                (int)Math.Ceiling(total / (double)request.PageSize)
            );

            return Result.Success(response);
        }
    }
}
