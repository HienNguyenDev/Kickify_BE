using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler : IQueryHandler<GetWalletTransactionsQuery, GetWalletTransactionsQueryResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUserContext _userContext;

    public GetWalletTransactionsQueryHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetWalletTransactionsQueryResponse>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<GetWalletTransactionsQueryResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        var wallet = await _walletRepository.GetByUserIdAsync(user.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetWalletTransactionsQueryResponse>(WalletErrors.WalletNotFound);
        }

        var (transactions, total) = await _walletTransactionRepository.GetByWalletIdAsync(
            wallet.WalletId,
            request.TransactionType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new WalletTransactionDto
        {
            TransactionId = t.TransactionId,
            TransactionType = t.TransactionType.ToString(),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            TransactionCode = t.TransactionCode,
            ReferenceId = t.ReferenceId,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(new GetWalletTransactionsQueryResponse
        {
            WalletId = wallet.WalletId,
            WalletType = wallet.WalletType.ToString(),
            Transactions = transactionDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }
}
