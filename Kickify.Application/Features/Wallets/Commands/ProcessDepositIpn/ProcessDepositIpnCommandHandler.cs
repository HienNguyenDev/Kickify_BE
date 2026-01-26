using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;

public class ProcessDepositIpnCommandHandler : ICommandHandler<ProcessDepositIpnCommand, ProcessDepositIpnCommandResponse>
{
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IPlayerWalletRepository _playerWalletRepository;
    private readonly IVenueWalletRepository _venueWalletRepository;
    private readonly IPlayerWalletTransactionRepository _playerWalletTransactionRepository;
    private readonly IVenueWalletTransactionRepository _venueWalletTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessDepositIpnCommandHandler> _logger;

    public ProcessDepositIpnCommandHandler(
        IPaymentRequestRepository paymentRequestRepository,
        IPlayerWalletRepository playerWalletRepository,
        IVenueWalletRepository venueWalletRepository,
        IPlayerWalletTransactionRepository playerWalletTransactionRepository,
        IVenueWalletTransactionRepository venueWalletTransactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessDepositIpnCommandHandler> logger)
    {
        _paymentRequestRepository = paymentRequestRepository;
        _playerWalletRepository = playerWalletRepository;
        _venueWalletRepository = venueWalletRepository;
        _playerWalletTransactionRepository = playerWalletTransactionRepository;
        _venueWalletTransactionRepository = venueWalletTransactionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProcessDepositIpnCommandResponse>> Handle(
        ProcessDepositIpnCommand request,
        CancellationToken cancellationToken)
    {
        var callback = request.CallbackData;

        _logger.LogInformation("Processing IPN for TxnRef: {TxnRef}", callback.TxnRef);

        // 1. Find PaymentRequest
        var paymentRequest = await _paymentRequestRepository
            .GetByTxnRefAsync(callback.TxnRef, cancellationToken);

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

        // 2. Check already processed
        if (paymentRequest.Status != PaymentStatus.Pending)
        {
            _logger.LogInformation("Already processed: {TxnRef}, Status: {Status}",
                callback.TxnRef, paymentRequest.Status);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = true,
                RspCode = "00",
                Message = "Order already confirmed"
            });
        }

        // 3. Verify amount
        if (paymentRequest.Amount != callback.Amount)
        {
            _logger.LogWarning(
                "Amount mismatch: {TxnRef}. Expected: {Expected}, Got: {Got}",
                callback.TxnRef, paymentRequest.Amount, callback.Amount);
            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "04",
                Message = "Invalid amount"
            });
        }

        // 4. Check payment success
        if (!callback.IsSuccess)
        {
            paymentRequest.Status = PaymentStatus.Failed;
            _paymentRequestRepository.Update(paymentRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment failed: {TxnRef}, Code: {Code}",
                callback.TxnRef, callback.ResponseCode);

            return Result.Success(new ProcessDepositIpnCommandResponse
            {
                Success = false,
                RspCode = "00",
                Message = "Payment failed"
            });
        }

        // 5. Process deposit
        try
        {
            if (paymentRequest.UserRole == UserRole.Player)
            {
                await ProcessPlayerDeposit(paymentRequest, callback);
            }
            else if (paymentRequest.UserRole == UserRole.VenueOwner)
            {
                await ProcessVenueOwnerDeposit(paymentRequest, callback);
            }

            // 6. Update PaymentRequest
            paymentRequest.Status = PaymentStatus.Completed;
            paymentRequest.VnpayTransactionNo = callback.VnpayTransactionId.ToString();
            paymentRequest.CompletedAt = DateTime.UtcNow;
            _paymentRequestRepository.Update(paymentRequest);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deposit success: TxnRef={TxnRef}, User={UserId}, Amount={Amount}",
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

    private async Task ProcessPlayerDeposit(
        PaymentRequest paymentRequest,
        VnPayCallbackData callback)
    {
        var wallet = await _playerWalletRepository
            .GetByIdAsync(paymentRequest.WalletId);

        if (wallet is null)
            throw new InvalidOperationException($"Wallet not found: {paymentRequest.WalletId}");

        wallet.Balance += paymentRequest.Amount;
        _playerWalletRepository.Update(wallet);

        var transaction = new PlayerWalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            PlayerWalletId = wallet.PlayerWalletId,
            TransactionType = TransactionType.Deposit,
            Amount = paymentRequest.Amount,
            BalanceAfter = wallet.Balance,
            TransactionCode = callback.VnpayTransactionId.ToString(),
            ReferenceId = paymentRequest.PaymentRequestId,
            Description = $"Deposit by VNPay - {callback.BankCode}",
            CreatedAt = DateTime.UtcNow
        };

        await _playerWalletTransactionRepository.AddAsync(transaction);
    }

    private async Task ProcessVenueOwnerDeposit(
        PaymentRequest paymentRequest,
        VnPayCallbackData callback)
    {
        var wallet = await _venueWalletRepository
            .GetByIdAsync(paymentRequest.WalletId);

        if (wallet is null)
            throw new InvalidOperationException($"Wallet not found: {paymentRequest.WalletId}");

        wallet.Balance += paymentRequest.Amount;
        _venueWalletRepository.Update(wallet);

        var transaction = new VenueWalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            VenueWalletId = wallet.VenueWalletId,
            TransactionType = TransactionType.Deposit,
            Amount = paymentRequest.Amount,
            BalanceAfter = wallet.Balance,
            TransactionCode = callback.VnpayTransactionId.ToString(),
            ReferenceId = paymentRequest.PaymentRequestId,
            Description = $"Deposit by VNPay - {callback.BankCode}",
            CreatedAt = DateTime.UtcNow
        };

        await _venueWalletTransactionRepository.AddAsync(transaction);
    }
}
