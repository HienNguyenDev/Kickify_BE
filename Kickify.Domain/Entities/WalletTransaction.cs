using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class WalletTransaction
{
    public Guid TransactionId { get; set; }
    public Guid WalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? ReferenceId { get; set; } // booking_id or withdrawal_id
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public VenueWallet VenueWallet { get; set; } = null!;
}
