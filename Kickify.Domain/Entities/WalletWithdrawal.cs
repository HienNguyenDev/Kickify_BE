using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class WalletWithdrawal
{
    public Guid WithdrawalId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
    public string? AdminNotes { get; set; }

    public Wallet Wallet { get; set; } = null!;
    public User? ProcessedByAdmin { get; set; }
}
