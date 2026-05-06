using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Common;
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
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WithdrawalNotFound);

        if (withdrawal.Status != WithdrawalStatus.Pending && withdrawal.Status != WithdrawalStatus.Processing)
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WithdrawalNotPending);

        var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId);
        if (wallet is null)
            return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.WalletNotFound);

        withdrawal.ProcessedDate = DateTime.UtcNow;
        withdrawal.ProcessedByAdminId = _userContext.UserId;
        withdrawal.AdminNotes = request.AdminNotes;

        // Declare at method scope so they are accessible in the return statement
        decimal fee = 0;
        decimal payoutAmount = 0;

        if (request.IsApproved)
        {
            if (wallet.Balance < withdrawal.Amount)
                return Result.Failure<ProcessWithdrawalCommandResponse>(WalletErrors.InsufficientBalance);

            var shouldChargeWithdrawalFee = wallet.WalletType == WalletType.VenueOwner;

            // Only venue owners pay the withdrawal fee.
            fee = shouldChargeWithdrawalFee
                ? Math.Min(
                    Math.Round(withdrawal.Amount * PlatformConstants.WithdrawalFeeRate, 0),
                    PlatformConstants.WithdrawalFeeCap)
                : 0;
            payoutAmount = withdrawal.Amount - fee;

            wallet.Balance -= withdrawal.Amount;
            withdrawal.Status = WithdrawalStatus.Completed;

            var withdrawalTx = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TransactionType = TransactionType.Withdrawal,
                Amount = -withdrawal.Amount,
                BalanceAfter = wallet.Balance,
                ReferenceId = withdrawal.WithdrawalId,
                Description = $"Withdrawal approved - payout {payoutAmount:N0} VND (fee {fee:N0} VND) - {wallet.BankName} - {wallet.BankAccountNumber}",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepository.AddAsync(withdrawalTx);

            if (fee > 0)
            {
                var feeTx = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    WalletId = wallet.WalletId,
                    TransactionType = TransactionType.WithdrawalFee,
                    Amount = -fee,
                    BalanceAfter = wallet.Balance,
                    ReferenceId = withdrawal.WithdrawalId,
                    Description = "Withdrawal fee 1% (max 50,000 VND)",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(feeTx);
            }

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
            NewBalance = request.IsApproved ? wallet.Balance : null,
            WithdrawalFee = request.IsApproved ? fee : null,
            PayoutAmount = request.IsApproved ? payoutAmount : null
        });
    }
}
