namespace Kickify.Application.Features.Premium.Commands.PurchasePremiumByWallet;

public record PurchasePremiumByWalletResponse(
    decimal Amount,
    decimal NewBalance,
    bool IsPremium,
    DateTime PremiumExpireAt,
    string Message);