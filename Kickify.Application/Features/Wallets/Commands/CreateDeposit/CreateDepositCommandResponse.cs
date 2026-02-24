namespace Kickify.Application.Features.Wallets.Commands.CreateDeposit;

public class CreateDepositCommandResponse
{
    public string PaymentUrl { get; set; } = null!;
    public string TxnRef { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime ExpiredAt { get; set; }
}
