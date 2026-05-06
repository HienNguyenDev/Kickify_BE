using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.DTOs;
using Kickify.Application.Features.Bookings.Commands.ProcessPayment;
using Kickify.Application.Common;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;

public class ProcessDepositIpnCommandHandler : ICommandHandler<ProcessDepositIpnCommand, ProcessDepositIpnCommandResponse>
{
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ISender _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDepositIpnCommandHandler> _logger;

    public ProcessDepositIpnCommandHandler(
        IPaymentRequestRepository paymentRequestRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        ISender mediator,
        IUnitOfWork unitOfWork,
        ILogger<ProcessDepositIpnCommandHandler> logger)
    {
        _paymentRequestRepository = paymentRequestRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _mediator = mediator;
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
                Success = false, RspCode = "01", Message = "Order not found"
            });
        }

        if (paymentRequest.Status != PaymentStatus.Pending)
        {
            _logger.LogInformation("Already processed: {TxnRef}, Status: {Status}", callback.TxnRef, paymentRequest.Status);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = true, RspCode = "00", Message = "Order already confirmed"
            });
        }

        if (paymentRequest.Amount != callback.Amount)
        {
            _logger.LogWarning("Amount mismatch: {TxnRef}. Expected: {Expected}, Got: {Got}",
                callback.TxnRef, paymentRequest.Amount, callback.Amount);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false, RspCode = "04", Message = "Invalid amount"
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
                Success = false, RspCode = "00", Message = "Payment failed"
            });
        }

        try
        {
            // ── Branch by purpose ──────────────────────────────────────────
            if (paymentRequest.Purpose == PaymentPurpose.CheckIn)
            {
                return await ProcessCheckInAsync(paymentRequest, callback, cancellationToken);
            }

                if (paymentRequest.Purpose == PaymentPurpose.PremiumPurchase)
                {
                    return await ProcessPremiumPurchaseAsync(paymentRequest, callback, cancellationToken);
                }

            // Wallet deposit (original path)
            await ProcessWalletDepositAsync(paymentRequest, callback);

            paymentRequest.Status = PaymentStatus.Completed;
            paymentRequest.VnpayTransactionNo = callback.VnpayTransactionId.ToString();
            paymentRequest.CompletedAt = DateTime.UtcNow;
            _paymentRequestRepository.Update(paymentRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deposit success: TxnRef={TxnRef}, User={UserId}, Amount={Amount}",
                callback.TxnRef, paymentRequest.UserId, paymentRequest.Amount);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = true, RspCode = "00", Message = "Confirm Success"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment: {TxnRef}", callback.TxnRef);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false, RspCode = "99", Message = "Unknown error"
            });
        }
    }

    // ── Check-in via VNPay ────────────────────────────────────────────────────
    private async Task<Result<ProcessDepositIpnCommandResponse>> ProcessCheckInAsync(
        PaymentRequest paymentRequest,
        VnPayCallbackData callback,
        CancellationToken cancellationToken)
    {
        if (paymentRequest.RoomId is null)
        {
            _logger.LogError("CheckIn payment {TxnRef} has no RoomId", callback.TxnRef);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false, RspCode = "99", Message = "CheckIn payment missing RoomId"
            });
        }

        // Mark PaymentRequest completed first (idempotency guard)
        paymentRequest.Status = PaymentStatus.Completed;
        paymentRequest.VnpayTransactionNo = callback.VnpayTransactionId.ToString();
        paymentRequest.CompletedAt = DateTime.UtcNow;
        _paymentRequestRepository.Update(paymentRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch the VNPay-specific check-in command (bypasses wallet balance check)
        var checkInCommand = new ProcessPaymentVnPayCommand(
            paymentRequest.RoomId.Value,
            paymentRequest.UserId,
            paymentRequest.Amount,
            callback.VnpayTransactionId.ToString());

        var result = await _mediator.Send(checkInCommand, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError(
                "CheckIn failed after VNPay confirmed. TxnRef={TxnRef}, Error={Error}",
                callback.TxnRef, result.Error.Description);

            // VNPay already charged the user — refund to their wallet
            await RefundToWalletAsync(paymentRequest, callback,
                $"Hoan tien check in that bai - {callback.BankCode}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "99",
                Message = $"Check-in failed: {result.Error.Description}"
            });
        }

        _logger.LogInformation(
            "CheckIn via VNPay success: TxnRef={TxnRef}, Room={RoomId}, User={UserId}",
            callback.TxnRef, paymentRequest.RoomId, paymentRequest.UserId);

        return Result.Success(new ProcessDepositIpnCommandResponse
        {
            Success = true, RspCode = "00", Message = "CheckIn Confirm Success"
        });
    }

    // ── Wallet deposit (original) ─────────────────────────────────────────────
    private async Task ProcessWalletDepositAsync(PaymentRequest paymentRequest, VnPayCallbackData callback)
    {
        var wallet = await _walletRepository.GetByIdAsync(paymentRequest.WalletId)
            ?? throw new InvalidOperationException($"Wallet not found: {paymentRequest.WalletId}");

        wallet.Balance += paymentRequest.Amount;
        _walletRepository.Update(wallet);

        await _walletTransactionRepository.AddAsync(new WalletTransaction
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
        });
    }

    // ── Refund helper ─────────────────────────────────────────────────────────
    private async Task RefundToWalletAsync(PaymentRequest paymentRequest, VnPayCallbackData callback, string description)
    {
        var wallet = await _walletRepository.GetByIdAsync(paymentRequest.WalletId);
        if (wallet is null) return;

        wallet.Balance += paymentRequest.Amount;
        _walletRepository.Update(wallet);

        await _walletTransactionRepository.AddAsync(new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            TransactionType = TransactionType.Refund,
            Amount = paymentRequest.Amount,
            BalanceAfter = wallet.Balance,
            TransactionCode = callback.VnpayTransactionId.ToString(),
            ReferenceId = paymentRequest.PaymentRequestId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

        // ── Premium purchase ──────────────────────────────────────────────────────
        private async Task<Result<ProcessDepositIpnCommandResponse>> ProcessPremiumPurchaseAsync(
            PaymentRequest paymentRequest,
            VnPayCallbackData callback,
            CancellationToken cancellationToken)
        {
            // Mark PaymentRequest completed
            paymentRequest.Status = PaymentStatus.Completed;
            paymentRequest.VnpayTransactionNo = callback.VnpayTransactionId.ToString();
            paymentRequest.CompletedAt = DateTime.UtcNow;
            _paymentRequestRepository.Update(paymentRequest);

            // Activate Premium on User
            var wallet = await _walletRepository.GetByIdAsync(paymentRequest.WalletId);
            if (wallet is null)
            {
                _logger.LogError("Premium IPN: wallet {WalletId} not found for user {UserId}", paymentRequest.WalletId, paymentRequest.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(new ProcessDepositIpnCommandResponse { Success = false, RspCode = "99", Message = "Wallet not found" });
            }

            // Record the premium purchase transaction (informational – no wallet balance change)
            await _walletTransactionRepository.AddAsync(new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TransactionType = TransactionType.PremiumPurchase,
                Amount = -paymentRequest.Amount,
                BalanceAfter = wallet.Balance,
                TransactionCode = callback.VnpayTransactionId.ToString(),
                ReferenceId = paymentRequest.PaymentRequestId,
                Description = $"Kickify Premium 30 days - VNPay {callback.BankCode}",
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Activate premium via a MediatR command so User is updated properly
            var activateCommand = new Kickify.Application.Features.Premium.Commands.ActivatePremium.ActivatePremiumCommand(paymentRequest.UserId);
            await _mediator.Send(activateCommand, cancellationToken);

            _logger.LogInformation("Premium activated for user {UserId} via VNPay TxnRef={TxnRef}", paymentRequest.UserId, callback.TxnRef);

            return Result.Success(new ProcessDepositIpnCommandResponse { Success = true, RspCode = "00", Message = "Premium activated" });
        }
}
