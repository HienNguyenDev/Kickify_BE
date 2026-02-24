namespace Kickify.Application.Features.Wallets.Queries.GetMyBankInfo;

public class GetMyBankInfoQueryResponse
{
    public Guid WalletId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public bool HasBankInfo { get; set; }
}
