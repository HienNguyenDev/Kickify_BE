using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;

public class ProcessWithdrawalCommandHandler : ICommandHandler<ProcessWithdrawalCommand, ProcessWithdrawalCommandResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletWithdrawalRepository _withdrawalRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessWithdrawalCommandHandler(
        IWalletRepository walletRepository,
        IWalletWithdrawalRepository withdrawalRepository,
        IWalletTransactionRepository transactionRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcessWithdrawalCommandResponse>> Handle(
        ProcessWithdrawalCommand request,
        CancellationToken cancellationToken)
    {
        var withdrawal = await _withdrawalRepository.GetByIdAsync(request.WithdrawalId);
        if (withdrawal is null)
        {
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WithdrawalNotFound);
        }

        if (withdrawal.Status != WithdrawalStatus.Pending && withdrawal.Status != WithdrawalStatus.Processing)
        {
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WithdrawalNotPending);
        }

        var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
        if (wallet is null)
        {
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WalletNotFound);
        }

        withdrawal.ProcessedDate = DateTime.UtcNow;
        withdrawal.ProcessedByAdminId = _userContext.UserId;
        withdrawal.AdminNotes = request.AdminNotes;

        if (request.IsApproved)
        {
            if (wallet.Balance < withdrawal.Amount)
            {
                return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.InsufficientBalance);
            }

            wallet.Balance -= withdrawal.Amount;
            withdrawal.Status = WithdrawalStatus.Completed;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TransactionType = TransactionType.Withdrawal,
                Amount = -withdrawal.Amount,
                BalanceAfter = wallet.Balance,
                ReferenceId = withdrawal.WithdrawalId,
                Description = $"Withdrawal approved - {wallet.BankName} - {wallet.BankAccountNumber}",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            _walletRepository.Update(wallet);
        }
        else
        {
            withdrawal.Status = WithdrawalStatus.Rejected;
        }

        _withdrawalRepository.Update(withdrawal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessWithdrawalCommandResponse
        {
            WithdrawalId = withdrawal.WithdrawalId,
            Status = withdrawal.Status.ToString(),
            Message = request.IsApproved 
                ? "Withdrawal approved and processed successfully" 
                : "Withdrawal rejected",
            NewBalance = request.IsApproved ? wallet.Balance : null
        });
    }
}
