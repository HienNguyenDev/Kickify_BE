namespace Kickify.Application.Features.Bookings.Queries.GetVenueOwnerBookings;

public record GetVenueOwnerBookingsResponse(
    List<VenueOwnerBookingItemDto> Bookings,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record VenueOwnerBookingItemDto(
    Guid BookingId,
    Guid RoomId,
    Guid FieldId,
    string FieldName,
    Guid VenueId,
    string VenueName,
    DateTime BookingDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal TotalAmount,
    decimal PlatformFee,
    decimal VenueAmount,
    string Status,
    string? PaymentMethod,
    DateTime CreatedAt
);
