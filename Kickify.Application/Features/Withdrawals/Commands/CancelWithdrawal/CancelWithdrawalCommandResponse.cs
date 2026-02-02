namespace Kickify.Application.Features.Withdrawals.Commands.CancelWithdrawal;

public class CancelWithdrawalCommandResponse
{
    public Guid WithdrawalId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
