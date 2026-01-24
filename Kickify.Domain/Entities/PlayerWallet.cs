using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class PlayerWallet : BaseEntity
{
    public Guid PlayerWalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; } = 0;
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<PlayerWalletTransaction> Transactions { get; set; } = new List<PlayerWalletTransaction>();
    public ICollection<PlayerWithdrawal> Withdrawals { get; set; } = new List<PlayerWithdrawal>();
}
