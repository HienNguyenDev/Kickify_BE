namespace Kickify.Application.Features.Wallets.Queries.GetAllWalletTransactions;

public class GetAllWalletTransactionsQueryResponse
{
    public IEnumerable<WalletTransactionItemDto> Transactions { get; set; } = new List<WalletTransactionItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WalletTransactionItemDto
{
    public Guid TransactionId { get; set; }
    public Guid WalletId { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserFullName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? TransactionCode { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
