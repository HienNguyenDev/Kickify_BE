using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Withdrawal
{
    public Guid WithdrawalId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? AdminNotes { get; set; }

    // Navigation properties
    public VenueWallet VenueWallet { get; set; } = null!;
}
