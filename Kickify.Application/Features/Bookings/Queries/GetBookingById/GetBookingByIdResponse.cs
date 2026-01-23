namespace Kickify.Application.Features.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdResponse(
        Guid BookingId,
        Guid RoomId,
        Guid FieldId,
        string FieldName,
        Guid VenueId,
        string VenueName,
        string VenueAddress,
        DateTime BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal TotalAmount,
        decimal PlatformFee,
        decimal VenueAmount,
        string Status,
        string? PaymentMethod,
        string? TransactionReference,
        DateTime CreatedAt
    );
}
