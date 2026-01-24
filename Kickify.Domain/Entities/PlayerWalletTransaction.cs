using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class PlayerWalletTransaction
{
    public Guid TransactionId { get; set; }
    public Guid PlayerWalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PlayerWallet PlayerWallet { get; set; } = null!;
}
