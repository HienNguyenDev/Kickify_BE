using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Bookings.Queries.GetVenueOwnerBookings;

public class GetVenueOwnerBookingsQueryHandler : IQueryHandler<GetVenueOwnerBookingsQuery, GetVenueOwnerBookingsResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetVenueOwnerBookingsQueryHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IUserContext userContext)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetVenueOwnerBookingsResponse>> Handle(
        GetVenueOwnerBookingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Verify user is a VenueOwner
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.VenueOwner)
        {
            return Result.Failure<GetVenueOwnerBookingsResponse>(BookingErrors.NotVenueOwner);
        }

        var (bookings, total) = await _bookingRepository.GetBookingsByVenueOwnerPagedAsync(
            userId,
            request.FieldId,
            request.Date,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var bookingItems = bookings.Select(b => new VenueOwnerBookingItemDto(
            b.BookingId,
            b.RoomId,
            b.FieldId,
            b.Field?.FieldName ?? string.Empty,
            b.Field?.VenueId ?? Guid.Empty,
            b.Field?.Venue?.VenueName ?? string.Empty,
            b.BookingDate,
            b.StartTime,
            b.EndTime,
            b.TotalAmount,
            b.PlatformFee,
            b.VenueAmount,
            b.Status.ToString(),
            b.PaymentMethod,
            b.CreatedAt
        )).ToList();

        var response = new GetVenueOwnerBookingsResponse(
            bookingItems,
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize)
        );

        return Result.Success(response);
    }
}
