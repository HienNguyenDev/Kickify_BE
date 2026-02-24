using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public WalletType WalletType { get; set; }
    public decimal Balance { get; set; } = 0;
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }

    public User User { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    public ICollection<WalletWithdrawal> Withdrawals { get; set; } = new List<WalletWithdrawal>();
}
