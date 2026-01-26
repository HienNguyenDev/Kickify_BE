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
    private readonly IPlayerWalletRepository _playerWalletRepository;
    private readonly IVenueWalletRepository _venueWalletRepository;
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IVnPayService _vnPayService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepositCommandHandler(
        IUserRepository userRepository,
        IPlayerWalletRepository playerWalletRepository,
        IVenueWalletRepository venueWalletRepository,
        IPaymentRequestRepository paymentRequestRepository,
        IVnPayService vnPayService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _playerWalletRepository = playerWalletRepository;
        _venueWalletRepository = venueWalletRepository;
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

        Guid walletId;
        if (user.Role == UserRole.Player)
        {
            var wallet = await _playerWalletRepository.GetByUserIdAsync(user.UserId, cancellationToken);
            if (wallet is null)
            {
                wallet = new PlayerWallet
                {
                    PlayerWalletId = Guid.NewGuid(),
                    UserId = user.UserId,
                    Balance = 0
                };
                await _playerWalletRepository.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            walletId = wallet.PlayerWalletId;
        }
        else if (user.Role == UserRole.VenueOwner)
        {
            var wallet = await _venueWalletRepository.GetByOwnerIdAsync(user.UserId, cancellationToken);
            if (wallet is null)
            {
                return Result.Failure<CreateDepositCommandResponse>(WalletErrors.WalletNotFound);
            }
            walletId = wallet.VenueWalletId;
        }
        else
        {
            return Result.Failure<CreateDepositCommandResponse>(WalletErrors.InvalidRole);
        }

        // 4. Create payment URL
        var (paymentUrl, txnRef) = _vnPayService.CreatePaymentUrl(
            request.Amount,
            "Nap tien vi Kickify"
        );

        // 5. Save PaymentRequest
        var expiredAt = DateTime.UtcNow.AddMinutes(15);
        var paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            TxnRef = txnRef,
            UserId = user.UserId,
            UserRole = user.Role,
            WalletId = walletId,
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
