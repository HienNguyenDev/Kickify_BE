using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Queries.GetMyBankInfo;

public class GetMyBankInfoQueryHandler : IQueryHandler<GetMyBankInfoQuery, GetMyBankInfoQueryResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUserContext _userContext;

    public GetMyBankInfoQueryHandler(
        IWalletRepository walletRepository,
        IUserContext userContext)
    {
        _walletRepository = walletRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyBankInfoQueryResponse>> Handle(
        GetMyBankInfoQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetMyBankInfoQueryResponse>(WalletErrors.WalletNotFound);
        }

        var hasBankInfo = !string.IsNullOrEmpty(wallet.BankAccountNumber) 
                          && !string.IsNullOrEmpty(wallet.BankName) 
                          && !string.IsNullOrEmpty(wallet.AccountHolderName);

        return Result.Success(new GetMyBankInfoQueryResponse
        {
            WalletId = wallet.WalletId,
            BankAccountNumber = wallet.BankAccountNumber,
            BankName = wallet.BankName,
            AccountHolderName = wallet.AccountHolderName,
            HasBankInfo = hasBankInfo
        });
    }
}
