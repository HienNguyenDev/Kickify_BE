using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class WalletErrors
{
    public static readonly Error WalletNotFound = Error.NotFound("Wallet.NotFound", "Wallet not found");
    public static readonly Error InsufficientBalance = Error.Conflict("Wallet.InsufficientBalance", "Insufficient balance");
    public static readonly Error MinimumDeposit = Error.Conflict("Wallet.MinimumDeposit", "Minimum deposit amount is 10,000 VND");
    public static readonly Error TransactionAlreadyProcessed = Error.Conflict("Wallet.TransactionAlreadyProcessed", "Transaction already processed");
    public static readonly Error InvalidPaymentData = Error.Conflict("Wallet.InvalidPaymentData", "Invalid payment data");
    public static readonly Error InvalidRole = Error.Conflict("Wallet.InvalidRole", "Invalid user role for deposit");
    
    public static readonly Error WithdrawalNotFound = Error.NotFound("Wallet.WithdrawalNotFound", "Withdrawal request not found");
    public static readonly Error HasPendingWithdrawal = Error.Conflict("Wallet.HasPendingWithdrawal", "You already have a pending withdrawal request");
    public static readonly Error MinimumWithdrawal = Error.Conflict("Wallet.MinimumWithdrawal", "Minimum withdrawal amount is 50,000 VND");
    public static readonly Error BankInfoRequired = Error.Conflict("Wallet.BankInfoRequired", "Bank account information is required for withdrawal");
    public static readonly Error WithdrawalNotPending = Error.Conflict("Wallet.WithdrawalNotPending", "Only pending withdrawals can be processed");
    public static readonly Error WithdrawalNotCancellable = Error.Conflict("Wallet.WithdrawalNotCancellable", "Only pending withdrawals can be cancelled");
    public static readonly Error NotWithdrawalOwner = Error.Conflict("Wallet.NotWithdrawalOwner", "You are not the owner of this withdrawal request");
}
