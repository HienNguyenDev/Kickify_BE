namespace Kickify.Application.Features.Wallets.Queries.GetWalletBalance;

public class GetWalletBalanceQueryResponse
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public bool IsBankVerified { get; set; }
}
