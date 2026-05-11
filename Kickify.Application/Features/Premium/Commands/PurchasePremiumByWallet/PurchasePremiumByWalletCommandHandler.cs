using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Common;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Premium.Commands.PurchasePremiumByWallet;

public class PurchasePremiumByWalletCommandHandler : ICommandHandler<PurchasePremiumByWalletCommand, PurchasePremiumByWalletResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public PurchasePremiumByWalletCommandHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchasePremiumByWalletResponse>> Handle(
        PurchasePremiumByWalletCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure<PurchasePremiumByWalletResponse>(UserErrors.NotFound(userId));

        var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
            return Result.Failure<PurchasePremiumByWalletResponse>(WalletErrors.WalletNotFound);

        if (wallet.Balance < PlatformConstants.PremiumPriceVnd)
            return Result.Failure<PurchasePremiumByWalletResponse>(WalletErrors.InsufficientBalance);

        wallet.Balance -= PlatformConstants.PremiumPriceVnd;

        var now = DateTime.UtcNow;
        var baseDate = user.IsPremium && user.PremiumExpireAt.HasValue && user.PremiumExpireAt > now
            ? user.PremiumExpireAt.Value
            : now;

        user.IsPremium = true;
        user.PremiumExpireAt = baseDate.Add(PlatformConstants.PremiumDuration);

        await _walletTransactionRepository.AddAsync(new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            TransactionType = TransactionType.PremiumPurchase,
            Amount = -PlatformConstants.PremiumPriceVnd,
            BalanceAfter = wallet.Balance,
            ReferenceId = user.UserId,
            TransactionCode = $"PREMIUM-WALLET-{Guid.NewGuid():N}",
            Description = "Kickify Premium 30 days - Wallet payment",
            CreatedAt = now
        });

        _walletRepository.Update(wallet);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new PurchasePremiumByWalletResponse(
            PlatformConstants.PremiumPriceVnd,
            wallet.Balance,
            user.IsPremium,
            user.PremiumExpireAt.Value,
            "Premium activated successfully using wallet balance"));
    }
}