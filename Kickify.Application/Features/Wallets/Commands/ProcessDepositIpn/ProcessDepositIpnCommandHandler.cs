using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.DTOs;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;

public class ProcessDepositIpnCommandHandler : ICommandHandler<ProcessDepositIpnCommand, ProcessDepositIpnCommandResponse>
{
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDepositIpnCommandHandler> _logger;

    public ProcessDepositIpnCommandHandler(
        IPaymentRequestRepository paymentRequestRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessDepositIpnCommandHandler> logger)
    {
        _paymentRequestRepository = paymentRequestRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProcessDepositIpnCommandResponse>> Handle(
        ProcessDepositIpnCommand request,
        CancellationToken cancellationToken)
    {
        var callback = request.CallbackData;

        _logger.LogInformation("Processing IPN for TxnRef: {TxnRef}", callback.TxnRef);

        var paymentRequest = await _paymentRequestRepository.GetByTxnRefAsync(callback.TxnRef, cancellationToken);

        if (paymentRequest == null)
        {
            _logger.LogWarning("PaymentRequest not found: {TxnRef}", callback.TxnRef);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "01",
                Message = "Order not found"
            });
        }

        if (paymentRequest.Status != PaymentStatus.Pending)
        {
            _logger.LogInformation("Already processed: {TxnRef}, Status: {Status}", callback.TxnRef, paymentRequest.Status);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = true,
                RspCode = "00",
                Message = "Order already confirmed"
            });
        }

        if (paymentRequest.Amount != callback.Amount)
        {
            _logger.LogWarning("Amount mismatch: {TxnRef}. Expected: {Expected}, Got: {Got}",
                callback.TxnRef, paymentRequest.Amount, callback.Amount);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "04",
                Message = "Invalid amount"
            });
        }

        if (!callback.IsSuccess)
        {
            paymentRequest.Status = PaymentStatus.Failed;
            _paymentRequestRepository.Update(paymentRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment failed: {TxnRef}, Code: {Code}", callback.TxnRef, callback.ResponseCode);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "00",
                Message = "Payment failed"
            });
        }

        try
        {
            await ProcessDeposit(paymentRequest, callback);

            paymentRequest.Status = PaymentStatus.Completed;
            paymentRequest.VnpayTransactionNo = callback.VnpayTransactionId.ToString();
            paymentRequest.CompletedAt = DateTime.UtcNow;
            _paymentRequestRepository.Update(paymentRequest);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deposit success: TxnRef={TxnRef}, User={UserId}, Amount={Amount}",
                callback.TxnRef, paymentRequest.UserId, paymentRequest.Amount);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = true,
                RspCode = "00",
                Message = "Confirm Success"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit: {TxnRef}", callback.TxnRef);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "99",
                Message = "Unknown error"
            });
        }
    }

    private async Task ProcessDeposit(PaymentRequest paymentRequest, VnPayCallbackData callback)
    {
        var wallet = await _walletRepository.GetByIdAsync(paymentRequest.WalletId);

        if (wallet is null)
            throw new InvalidOperationException($"Wallet not found: {paymentRequest.WalletId}");

        wallet.Balance += paymentRequest.Amount;
        _walletRepository.Update(wallet);

        var transaction = new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            TransactionType = TransactionType.Deposit,
            Amount = paymentRequest.Amount,
            BalanceAfter = wallet.Balance,
            TransactionCode = callback.VnpayTransactionId.ToString(),
            ReferenceId = paymentRequest.PaymentRequestId,
            Description = $"Deposit by VNPay - {callback.BankCode}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletTransactionRepository.AddAsync(transaction);
    }
}
