using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Commands.UpdateBankInfo;

public class UpdateBankInfoCommandHandler : ICommandHandler<UpdateBankInfoCommand, UpdateBankInfoCommandResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBankInfoCommandHandler(
        IWalletRepository walletRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateBankInfoCommandResponse>> Handle(
        UpdateBankInfoCommand request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<UpdateBankInfoCommandResponse>(WalletErrors.WalletNotFound);
        }

        wallet.BankAccountNumber = request.BankAccountNumber;
        wallet.BankName = request.BankName;
        wallet.AccountHolderName = request.AccountHolderName;
        wallet.IsBankVerified = false;

        _walletRepository.Update(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateBankInfoCommandResponse
        {
            WalletId = wallet.WalletId,
            BankAccountNumber = wallet.BankAccountNumber,
            BankName = wallet.BankName,
            AccountHolderName = wallet.AccountHolderName,
            IsBankVerified = wallet.IsBankVerified,
            Message = "Bank information updated successfully"
        });
    }
}
