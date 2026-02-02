using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Wallets.Queries.GetAllWalletTransactions;

public class GetAllWalletTransactionsQueryHandler : IQueryHandler<GetAllWalletTransactionsQuery, GetAllWalletTransactionsQueryResponse>
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;

    public GetAllWalletTransactionsQueryHandler(IWalletTransactionRepository walletTransactionRepository)
    {
        _walletTransactionRepository = walletTransactionRepository;
    }

    public async Task<Result<GetAllWalletTransactionsQueryResponse>> Handle(GetAllWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (transactions, total) = await _walletTransactionRepository.GetAllAsync(
            request.WalletType,
            request.TransactionType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new WalletTransactionItemDto
        {
            TransactionId = t.TransactionId,
            WalletId = t.WalletId,
            WalletType = t.Wallet?.WalletType.ToString() ?? "Unknown",
            UserId = t.Wallet?.UserId ?? Guid.Empty,
            UserFullName = t.Wallet?.User?.FullName,
            TransactionType = t.TransactionType.ToString(),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            TransactionCode = t.TransactionCode,
            ReferenceId = t.ReferenceId,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(new GetAllWalletTransactionsQueryResponse
        {
            Transactions = transactionDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }
}
