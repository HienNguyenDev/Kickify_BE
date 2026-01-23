namespace Kickify.Application.Features.Bookings.Queries.GetAllBookings
{
    public record GetAllBookingsResponse(
        List<BookingItemDto> Bookings,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record BookingItemDto(
        Guid BookingId,
        Guid RoomId,
        Guid FieldId,
        string FieldName,
        string VenueName,
        DateTime BookingDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal TotalAmount,
        string Status,
        DateTime CreatedAt
    );
}
