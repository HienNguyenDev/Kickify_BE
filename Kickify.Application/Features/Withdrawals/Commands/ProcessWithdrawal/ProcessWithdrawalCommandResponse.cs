namespace Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;

public class ProcessWithdrawalCommandResponse
{
    public Guid WithdrawalId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal? NewBalance { get; set; }
    /// <summary>1 % fee retained by the platform.</summary>
    public decimal? WithdrawalFee { get; set; }
    /// <summary>Actual amount transferred to the owner's bank account.</summary>
    public decimal? PayoutAmount { get; set; }
}
