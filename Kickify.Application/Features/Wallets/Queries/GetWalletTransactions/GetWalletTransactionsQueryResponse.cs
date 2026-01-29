namespace Kickify.Application.Features.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryResponse
{
    public Guid WalletId { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public IEnumerable<WalletTransactionDto> Transactions { get; set; } = new List<WalletTransactionDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WalletTransactionDto
{
    public Guid TransactionId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? TransactionCode { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
