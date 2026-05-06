namespace Kickify.Application.Features.Premium.Commands.PurchasePremium;

public record PurchasePremiumResponse(
    string PaymentUrl,
    string TxnRef,
    decimal Amount,
    DateTime ExpiredAt);
