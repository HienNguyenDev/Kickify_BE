using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQuery : IQuery<GetWalletTransactionsQueryResponse>
{
    public TransactionType? TransactionType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
