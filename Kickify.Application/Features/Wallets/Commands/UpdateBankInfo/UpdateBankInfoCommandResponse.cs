namespace Kickify.Application.Features.Wallets.Commands.UpdateBankInfo;

public class UpdateBankInfoCommandResponse
{
    public Guid WalletId { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public bool IsBankVerified { get; set; }
    public string Message { get; set; } = string.Empty;
}
