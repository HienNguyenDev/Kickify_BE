using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class VenueWithdrawal
{
    public Guid VenueWithdrawalId { get; set; }
    public Guid VenueWalletId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
    public string? AdminNotes { get; set; }

    // Navigation properties
    public VenueWallet VenueWallet { get; set; } = null!;
    public User? ProcessedByAdmin { get; set; }
}
