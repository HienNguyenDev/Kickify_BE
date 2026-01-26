using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class VenueWalletTransaction
{
    public Guid TransactionId { get; set; }
    public Guid VenueWalletId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? TransactionCode { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public VenueWallet VenueWallet { get; set; } = null!;
}
