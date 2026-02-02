using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Withdrawals.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandHandler : ICommandHandler<CreateWithdrawalCommand, CreateWithdrawalCommandResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletWithdrawalRepository _withdrawalRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWithdrawalCommandHandler(
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

    public async Task<Result<CreateWithdrawalCommandResponse>> Handle(
        CreateWithdrawalCommand request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(_userContext.UserId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<CreateWithdrawalCommandResponse>(WalletErrors.WalletNotFound);
        }

        if (string.IsNullOrEmpty(wallet.BankAccountNumber) || 
            string.IsNullOrEmpty(wallet.BankName) || 
            string.IsNullOrEmpty(wallet.AccountHolderName))
        {
            return Result.Failure<CreateWithdrawalCommandResponse>(WalletErrors.BankInfoRequired);
        }

        if (wallet.Balance < request.Amount)
        {
            return Result.Failure<CreateWithdrawalCommandResponse>(WalletErrors.InsufficientBalance);
        }

        var hasPending = await _withdrawalRepository.HasPendingWithdrawalAsync(wallet.WalletId, cancellationToken);
        if (hasPending)
        {
            return Result.Failure<CreateWithdrawalCommandResponse>(WalletErrors.HasPendingWithdrawal);
        }

        var withdrawal = new WalletWithdrawal
        {
            WithdrawalId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            Amount = request.Amount,
            Status = WithdrawalStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        await _withdrawalRepository.AddAsync(withdrawal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateWithdrawalCommandResponse
        {
            WithdrawalId = withdrawal.WithdrawalId,
            WalletId = withdrawal.WalletId,
            Amount = withdrawal.Amount,
            Status = withdrawal.Status.ToString(),
            RequestDate = withdrawal.RequestDate,
            BankAccountNumber = wallet.BankAccountNumber,
            BankName = wallet.BankName,
            AccountHolderName = wallet.AccountHolderName
        });
    }
}
