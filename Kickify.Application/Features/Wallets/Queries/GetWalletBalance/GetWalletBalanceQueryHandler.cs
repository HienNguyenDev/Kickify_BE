using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Queries.GetWalletBalance;

public class GetWalletBalanceQueryHandler : IQueryHandler<GetWalletBalanceQuery, GetWalletBalanceQueryResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUserContext _userContext;

    public GetWalletBalanceQueryHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetWalletBalanceQueryResponse>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<GetWalletBalanceQueryResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        var wallet = await _walletRepository.GetByUserIdAsync(user.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetWalletBalanceQueryResponse>(WalletErrors.WalletNotFound);
        }

        return Result.Success(new GetWalletBalanceQueryResponse
        {
            WalletId = wallet.WalletId,
            UserId = wallet.UserId,
            WalletType = wallet.WalletType.ToString(),
            Balance = wallet.Balance,
            BankAccountNumber = wallet.BankAccountNumber,
            BankName = wallet.BankName,
            AccountHolderName = wallet.AccountHolderName
        });
    }
}
