namespace Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;

public class ProcessWithdrawalCommandResponse
{
    public Guid WithdrawalId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal? NewBalance { get; set; }
}
