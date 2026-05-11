using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Common;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment;

/// <summary>
/// Handles check-in when payment arrived via VNPay directly (no wallet deduction).
/// Mirrors ProcessPaymentCommandHandler but:
///   - Accepts an explicit UserId (IPN has no HTTP user context)
///   - No wallet balance check / deduction
///   - Records a WalletTransaction of type CheckInFee with positive amount (VNPay-sourced)
/// </summary>
public class ProcessPaymentVnPayCommandHandler : ICommandHandler<ProcessPaymentVnPayCommand, ProcessPaymentResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMatchRoomHubService _matchRoomHubService;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentVnPayCommandHandler> _logger;

    public ProcessPaymentVnPayCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IBookingRepository bookingRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUserRepository userRepository,
        IMatchRoomHubService matchRoomHubService,
        IMatchLifecycleService matchLifecycleService,
        IRoomAutoCloseService roomAutoCloseService,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentVnPayCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _bookingRepository = bookingRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _userRepository = userRepository;
        _matchRoomHubService = matchRoomHubService;
        _matchLifecycleService = matchLifecycleService;
        _roomAutoCloseService = roomAutoCloseService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProcessPaymentResponse>> Handle(
        ProcessPaymentVnPayCommand request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userRepository.GetByIdAsync(request.UserId);
        if (currentUser is null)
            return Result.Failure<ProcessPaymentResponse>(UserErrors.NotFound(request.UserId));

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room is null)
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.RoomNotFound(request.RoomId));

        var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant is null)
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.ParticipantNotFound);

        if (participant.DepositPaid)
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.AlreadyPaid);

        var depositAmount = request.Amount;

        // ── Record a CheckInFee transaction (no wallet deduction — money came from VNPay) ──
        var playerWallet = await _walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (playerWallet is not null)
        {
            // Balance stays the same; we only record the transaction for audit trail.
            var playerTransaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = playerWallet.WalletId,
                TransactionType = TransactionType.CheckInFee,
                Amount = -depositAmount,        // negative = outflow, but came from VNPay
                BalanceAfter = playerWallet.Balance,  // balance unchanged
                ReferenceId = room.RoomId,
                TransactionCode = request.VnpayTransactionId,
                Description = $"VNPay check-in for room {room.RoomName ?? room.RoomId.ToString()}",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.AddAsync(playerTransaction);
        }

        // Mark participant paid
        participant.DepositPaid = true;
        participant.CheckedIn = true;
        participant.CheckInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        participant.DepositAmount = depositAmount;
        room.TotalDepositCollected += depositAmount;

        bool allPaid = room.FilledSlots >= room.TotalSlots
                       && room.RoomParticipants.All(p => p.DepositPaid);

        if (allPaid)
        {
            _logger.LogInformation("All participants paid (VNPay) for room {RoomId}. Confirming booking...", request.RoomId);

            if (room.Field is null)
                return Result.Failure<ProcessPaymentResponse>(FieldErrors.NotFound(room.FieldId));

            var booking = await _bookingRepository.GetBookingByRoomAsync(request.RoomId, cancellationToken);
            if (booking is null)
                return Result.Failure<ProcessPaymentResponse>(BookingErrors.NotFound(request.RoomId));

            try
            {
                booking.Field = room.Field;
                booking.Status = BookingStatus.Confirmed;
                    var totalAmount = room.RoomParticipants.Sum(p => p.DepositAmount ?? 0);
                    booking.TotalAmount = totalAmount;
                    booking.PlatformFee = Math.Round(totalAmount * PlatformConstants.BookingCommissionRate, 0);
                    booking.VenueAmount = totalAmount - booking.PlatformFee;
                _bookingRepository.Update(booking);

                room.Status = RoomStatus.Locked;
                _matchRoomRepository.Update(room);

                _roomAutoCloseService.CancelAutoClose(room.AutoCloseJobId);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var matchStartTime = room.MatchDate.Add(room.StartTime);
                _matchLifecycleService.ScheduleMatchStart(request.RoomId, matchStartTime);

                _logger.LogInformation(
                    "Booking {BookingId} confirmed (VNPay) for room {RoomId}. Room locked.",
                    booking.BookingId, request.RoomId);

                await _matchRoomHubService.NotifyParticipantPaidAsync(
                    request.RoomId, request.UserId,
                    currentUser.FullName ?? "Unknown",
                    depositAmount, room.TotalDepositCollected,
                    cancellationToken);

                await _matchRoomHubService.NotifyBookingCreatedAsync(
                    request.RoomId, booking.BookingId,
                    booking.BookingDate, booking.StartTime, booking.EndTime,
                    cancellationToken);

                await _matchRoomHubService.NotifyRoomStatusChangedAsync(
                    request.RoomId, room.Status.ToString(), cancellationToken);

                return Result.Success(new ProcessPaymentResponse(
                    true,
                    "VNPay check-in processed. Booking confirmed.",
                    booking.BookingId, booking.BookingDate, booking.StartTime, booking.EndTime));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay check-in for room {RoomId}", request.RoomId);
                throw;
            }
        }
        else
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} paid via VNPay for room {RoomId}. Waiting for other participants...",
                request.UserId, request.RoomId);

            await _matchRoomHubService.NotifyParticipantPaidAsync(
                request.RoomId, request.UserId,
                currentUser.FullName ?? "Unknown",
                depositAmount, room.TotalDepositCollected,
                cancellationToken);

            return Result.Success(new ProcessPaymentResponse(
                false,
                "VNPay payment recorded. Waiting for other participants.",
                null, null, null, null));
        }
    }
}
