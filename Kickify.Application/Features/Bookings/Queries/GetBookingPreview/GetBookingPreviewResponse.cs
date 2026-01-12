namespace Kickify.Application.Features.Bookings.Queries.GetBookingPreview
{
    public record GetBookingPreviewResponse(
        Guid FieldId,
        string FieldName,
        string VenueName,
        DateTime Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal DurationHours,
        decimal PricePerHour,
        decimal TotalAmount,
        int NumberOfPlayers,
        decimal SharePerPlayer,
        bool IsAvailable
    );
}
