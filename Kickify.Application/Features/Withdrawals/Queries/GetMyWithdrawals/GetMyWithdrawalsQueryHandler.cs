using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Withdrawals.Queries.GetMyWithdrawals;

public class GetMyWithdrawalsQueryHandler : IQueryHandler<GetMyWithdrawalsQuery, GetMyWithdrawalsQueryResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletWithdrawalRepository _withdrawalRepository;
    private readonly IUserContext _userContext;

    public GetMyWithdrawalsQueryHandler(
        IWalletRepository walletRepository,
        IWalletWithdrawalRepository withdrawalRepository,
        IUserContext userContext)
    {
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyWithdrawalsQueryResponse>> Handle(
        GetMyWithdrawalsQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetMyWithdrawalsQueryResponse>(WalletErrors.WalletNotFound);
        }

        var (withdrawals, total) = await _withdrawalRepository.GetByWalletIdAsync(
            wallet.WalletId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var withdrawalDtos = withdrawals.Select(w => new WithdrawalDto
        {
            WithdrawalId = w.WithdrawalId,
            Amount = w.Amount,
            Status = w.Status.ToString(),
            RequestDate = w.RequestDate,
            ProcessedDate = w.ProcessedDate,
            AdminNotes = w.AdminNotes,
            BankAccountNumber = w.Wallet.BankAccountNumber,
            BankName = w.Wallet.BankName,
            AccountHolderName = w.Wallet.AccountHolderName
        }).ToList();

        return Result.Success(new GetMyWithdrawalsQueryResponse
        {
            Withdrawals = withdrawalDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }
}
