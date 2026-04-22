using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Bookings.Commands.CreateCheckInPayment;

public class CreateCheckInPaymentCommandHandler
    : ICommandHandler<CreateCheckInPaymentCommand, CreateCheckInPaymentResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRequestRepository _paymentRequestRepository;
    private readonly IVnPayService _vnPayService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckInPaymentCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IPaymentRequestRepository paymentRequestRepository,
        IVnPayService vnPayService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _matchRoomRepository = matchRoomRepository;
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _paymentRequestRepository = paymentRequestRepository;
        _vnPayService = vnPayService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCheckInPaymentResponse>> Handle(
        CreateCheckInPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var user = await _userRepository.GetByIdAsync(currentUserId);
        if (user is null)
            return Result.Failure<CreateCheckInPaymentResponse>(UserErrors.NotFound(currentUserId));

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room is null)
            return Result.Failure<CreateCheckInPaymentResponse>(BookingErrors.RoomNotFound(request.RoomId));

        var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        if (participant is null)
            return Result.Failure<CreateCheckInPaymentResponse>(BookingErrors.ParticipantNotFound);

        if (participant.DepositPaid)
            return Result.Failure<CreateCheckInPaymentResponse>(BookingErrors.AlreadyPaid);

        var depositAmount = room.DepositPerPerson ?? 0;
        if (depositAmount <= 0)
            return Result.Failure<CreateCheckInPaymentResponse>(
                BookingErrors.NoDepositRequired(request.RoomId));

        // Ensure user has a wallet (for potential refund path later)
        var wallet = await _walletRepository.GetByUserIdAsync(currentUserId, cancellationToken);
        if (wallet is null)
        {
            var walletType = user.Role == UserRole.VenueOwner ? WalletType.VenueOwner : WalletType.Player;
            wallet = new Wallet
            {
                WalletId = Guid.NewGuid(),
                UserId = currentUserId,
                WalletType = walletType,
                Balance = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var (paymentUrl, txnRef) = _vnPayService.CreatePaymentUrl(
            depositAmount,
            $"Thanh toan check in phong {room.RoomName ?? request.RoomId.ToString()}");

        var expiredAt = DateTime.UtcNow.AddMinutes(15);
        var paymentRequest = new PaymentRequest
        {
            PaymentRequestId = Guid.NewGuid(),
            TxnRef = txnRef,
            UserId = currentUserId,
            WalletId = wallet.WalletId,
            Amount = depositAmount,
            Status = PaymentStatus.Pending,
            Purpose = PaymentPurpose.CheckIn,
            RoomId = request.RoomId,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt
        };

        await _paymentRequestRepository.AddAsync(paymentRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCheckInPaymentResponse(paymentUrl, txnRef, depositAmount, expiredAt));
    }
}
