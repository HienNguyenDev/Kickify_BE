using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Commands.CreateDeposit;

public class CreateDepositCommandHandler : ICommandHandler<CreateDepositCommand, CreateDepositCommandResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IVnPayService _vnPayService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepositCommandHandler(
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

    public async Task<Result<CreateDepositCommandResponse>> Handle(
        CreateDepositCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<CreateDepositCommandResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        var wallet = await _walletRepository.GetByUserIdAsync(user.UserId, cancellationToken);
        if (wallet is null)
        {
            var walletType = user.Role == UserRole.VenueOwner ? WalletType.VenueOwner : WalletType.Player;
            wallet = new Wallet
            {
                WalletId = Guid.NewGuid(),
                UserId = user.UserId,
                WalletType = walletType,
                Balance = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var (paymentUrl, txnRef) = _vnPayService.CreatePaymentUrl(
            request.Amount,
            "Nap tien vi Kickify"
        );

        var expiredAt = DateTime.UtcNow.AddMinutes(15);
        var paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            TxnRef = txnRef,
            UserId = user.UserId,
            WalletId = wallet.WalletId,
            Amount = request.Amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt
        };

        await _paymentRequestRepository.AddAsync(paymentRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateDepositCommandResponse
        {
            PaymentUrl = paymentUrl,
            TxnRef = txnRef,
            Amount = request.Amount,
            ExpiredAt = expiredAt
        };
        return Result.Success(response);
    }
}
