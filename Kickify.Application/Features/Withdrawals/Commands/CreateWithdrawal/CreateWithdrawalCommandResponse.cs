namespace Kickify.Application.Features.Withdrawals.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandResponse
{
    public Guid WithdrawalId { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
}
