namespace Kickify.Application.Features.Bookings.Commands.CreateCheckInPayment;

public record CreateCheckInPaymentResponse(
    string PaymentUrl,
    string TxnRef,
    decimal Amount,
    DateTime ExpiredAt
);
