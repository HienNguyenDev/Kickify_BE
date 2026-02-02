using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Withdrawals.Commands.CancelWithdrawal;

public class CancelWithdrawalCommandHandler : ICommandHandler<CancelWithdrawalCommand, CancelWithdrawalCommandResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletWithdrawalRepository _withdrawalRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CancelWithdrawalCommandHandler(
        IWalletRepository walletRepository,
        IWalletWithdrawalRepository withdrawalRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CancelWithdrawalCommandResponse>> Handle(
        CancelWithdrawalCommand request,
        CancellationToken cancellationToken)
    {
        var withdrawal = await _withdrawalRepository.GetByIdAsync(request.WithdrawalId);
        if (withdrawal is null)
        {
            return Result.Failure<CancelWithdrawalCommandResponse>(WalletErrors.WithdrawalNotFound);
        }

        var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
        if (wallet is null || wallet.UserId != _userContext.UserId)
        {
            return Result.Failure<CancelWithdrawalCommandResponse>(WalletErrors.NotWithdrawalOwner);
        }

        if (withdrawal.Status != WithdrawalStatus.Pending)
        {
            return Result.Failure<CancelWithdrawalCommandResponse>(WalletErrors.WithdrawalNotCancellable);
        }

        withdrawal.Status = WithdrawalStatus.Cancelled;
        _withdrawalRepository.Update(withdrawal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CancelWithdrawalCommandResponse
        {
            WithdrawalId = withdrawal.WithdrawalId,
            Status = withdrawal.Status.ToString(),
            Message = "Withdrawal request cancelled successfully"
        });
    }
}
