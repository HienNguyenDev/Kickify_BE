using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class PlayerWithdrawal
{
    public Guid PlayerWithdrawalId { get; set; }
    public Guid PlayerWalletId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
    public string? AdminNotes { get; set; }

    // Navigation properties
    public PlayerWallet PlayerWallet { get; set; } = null!;
    public User? ProcessedByAdmin { get; set; }
}
