using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class WalletErrors
{
    public static readonly Error WalletNotFound = Error.NotFound("Wallet.NotFound", "Wallet not found");
    public static readonly Error InsufficientBalance = Error.Conflict("Wallet.InsufficientBalance", "Insufficient balance");
    public static readonly Error MinimumDeposit = Error.Conflict("Wallet.MinimumDeposit", "Minimum deposit amount is 10,000?");
    public static readonly Error TransactionAlreadyProcessed = Error.Conflict("Wallet.TransactionAlreadyProcessed", "Transaction already processed");
    public static readonly Error InvalidPaymentData = Error.Problem("Wallet.InvalidPaymentData", "Invalid payment data");
    public static readonly Error InvalidRole = Error.Conflict("Wallet.InvalidRole", "Invalid user role for deposit");
}
