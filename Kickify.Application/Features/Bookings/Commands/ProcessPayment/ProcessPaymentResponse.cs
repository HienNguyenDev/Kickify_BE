namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment
{
    public record ProcessPaymentResponse(
        bool BookingCreated,
        string Message,
        Guid? BookingId,
        DateTime? BookingDate,
        TimeSpan? StartTime,
        TimeSpan? EndTime
    );
}
