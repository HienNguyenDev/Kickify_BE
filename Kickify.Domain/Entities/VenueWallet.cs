using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class VenueWallet : BaseEntity
{
    public Guid VenueWalletId { get; set; }
    public Guid VenueId { get; set; }
    public decimal Balance { get; set; } = 0;
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public bool IsBankVerified { get; set; } = false;

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    public ICollection<VenueWithdrawal> VenueWithdrawals { get; set; } = new List<VenueWithdrawal>();
}
