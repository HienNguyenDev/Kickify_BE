using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class WalletTransaction
{
    public Guid TransactionId { get; set; }
    public Guid WalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? TransactionCode { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
