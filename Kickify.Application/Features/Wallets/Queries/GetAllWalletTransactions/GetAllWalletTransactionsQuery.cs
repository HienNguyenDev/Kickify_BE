using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Wallets.Queries.GetAllWalletTransactions;

public class GetAllWalletTransactionsQuery : IQuery<GetAllWalletTransactionsQueryResponse>
{
    public WalletType? WalletType { get; set; }
    public TransactionType? TransactionType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
