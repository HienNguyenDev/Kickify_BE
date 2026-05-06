using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Common;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Premium.Commands.PurchasePremium;

public class PurchasePremiumCommandHandler : ICommandHandler<PurchasePremiumCommand, PurchasePremiumResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IVnPayService _vnPayService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public PurchasePremiumCommandHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IPaymentRequestRepository paymentRequestRepository,
        IVnPayService vnPayService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _paymentRequestRepository = paymentRequestRepository;
        _vnPayService = vnPayService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchasePremiumResponse>> Handle(
        PurchasePremiumCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure<PurchasePremiumResponse>(UserErrors.NotFound(userId));

        // Ensure user has a wallet for potential audit trail
        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
        {
            wallet = new Wallet
            {
                WalletId = Guid.NewGuid(),
                UserId = userId,
                WalletType = user.Role == UserRole.VenueOwner ? WalletType.VenueOwner : WalletType.Player,
                Balance = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var (paymentUrl, txnRef) = _vnPayService.CreatePaymentUrl(
            PlatformConstants.PremiumPriceVnd,
            "Kickify Premium 30 ngay");

        var expiredAt = DateTime.UtcNow.AddMinutes(15);

        var paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            TxnRef = txnRef,
            UserId = userId,
            WalletId = wallet.WalletId,
            Amount = PlatformConstants.PremiumPriceVnd,
            Status = PaymentStatus.Pending,
            Purpose = PaymentPurpose.PremiumPurchase,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt
        };

        await _paymentRequestRepository.AddAsync(paymentRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new PurchasePremiumResponse(
            paymentUrl,
            txnRef,
            PlatformConstants.PremiumPriceVnd,
            expiredAt));
    }
}
