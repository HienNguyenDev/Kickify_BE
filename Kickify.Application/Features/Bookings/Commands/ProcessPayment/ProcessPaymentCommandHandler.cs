using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMatchRoomHubService _matchRoomHubService;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IBookingRepository bookingRepository,
        IVenueRepository venueRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUserRepository userRepository,
        IMatchRoomHubService matchRoomHubService,
        IMatchLifecycleService matchLifecycleService,
        IRoomAutoCloseService roomAutoCloseService,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _bookingRepository = bookingRepository;
        _venueRepository = venueRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _userRepository = userRepository;
        _matchRoomHubService = matchRoomHubService;
        _matchLifecycleService = matchLifecycleService;
        _roomAutoCloseService = roomAutoCloseService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<ProcessPaymentResponse>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        // Get current user info for notifications
        var currentUser = await _userRepository.GetByIdAsync(currentUserId);
        if (currentUser == null)
        {
            return Result.Failure<ProcessPaymentResponse>(UserErrors.NotFound(currentUserId));
        }

        // Get room with participants WITH TRACKING for update operations
        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room == null)
        {
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.RoomNotFound(request.RoomId));
        }

        // Check if user is participant
        var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        if (participant == null)
        {
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.ParticipantNotFound);
        }

        // Check if already paid
        if (participant.DepositPaid)
        {
            return Result.Failure<ProcessPaymentResponse>(BookingErrors.AlreadyPaid);
        }

        var depositAmount = room.DepositPerPerson ?? 0;

        // Get player's wallet and validate balance
        var playerWallet = await _walletRepository.GetByUserIdAsync(currentUserId, cancellationToken);
        if (playerWallet == null)
        {
            return Result.Failure<ProcessPaymentResponse>(WalletErrors.WalletNotFound);
        }

        if (playerWallet.Balance < depositAmount)
        {
            return Result.Failure<ProcessPaymentResponse>(WalletErrors.InsufficientBalance);
        }

        // Deduct from player's wallet
        playerWallet.Balance -= depositAmount;
        _walletRepository.Update(playerWallet);

        // Create transaction record for player payment
        var playerTransaction = new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = playerWallet.WalletId,
            TransactionType = TransactionType.CheckInFee,
            Amount = -depositAmount,
            BalanceAfter = playerWallet.Balance,
            ReferenceId = room.RoomId,
            Description = $"Payment for room {room.RoomName ?? room.RoomId.ToString()}",
            CreatedAt = DateTime.UtcNow
        };
        await _walletTransactionRepository.AddAsync(playerTransaction);

        // Mark as paid, checked in, and update deposit amount
        participant.DepositPaid = true;
        participant.CheckedIn = true;
        participant.CheckInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        participant.DepositAmount = depositAmount;
        room.TotalDepositCollected += depositAmount;

        // Check if all participants have paid
        bool allPaid = room.FilledSlots >= room.TotalSlots
                       && room.RoomParticipants.All(p => p.DepositPaid);

        if (allPaid)
        {
            _logger.LogInformation("All participants paid for room {RoomId}. Creating booking...", request.RoomId);

            if (room.Field == null)
            {
                return Result.Failure<ProcessPaymentResponse>(FieldErrors.NotFound(room.FieldId));
            }

            var booking = await _bookingRepository.GetBookingByRoomAsync(request.RoomId, cancellationToken);
            if (booking == null)
            {
                return Result.Failure<ProcessPaymentResponse>(BookingErrors.NotFound(request.RoomId));
            }

            // Calculate total amount
            var totalAmount = room.RoomParticipants.Sum(p => p.DepositAmount ?? 0);

            try
            {
                // Reuse the Field instance already tracked on room (GetBookingByRoom uses AsNoTracking
                // and includes Field — Update(booking) would otherwise attach a second Field with the same key).
                booking.Field = room.Field;

                // Update booking status
                booking.Status = BookingStatus.Confirmed;
                _bookingRepository.Update(booking);

                // Transfer payment to venue owner's wallet
                //var venue = await _venueRepository.GetByIdAsync(field.VenueId);
                //if (venue != null)
                //{
                //    var wallet = await _walletRepository.GetByUserIdAsync(venue.OwnerId, cancellationToken);
                //    if (wallet != null)
                //    {
                //        wallet.Balance += totalAmount;
                //        _walletRepository.Update(wallet);

                //        var transaction = new WalletTransaction
                //        {
                //            TransactionId = Guid.NewGuid(),
                //            WalletId = wallet.WalletId,
                //            TransactionType = TransactionType.BookingIncome,
                //            Amount = totalAmount,
                //            BalanceAfter = wallet.Balance,
                //            ReferenceId = booking.BookingId,
                //            Description = $"Booking income from room {room.RoomName ?? room.RoomId.ToString()}",
                //            CreatedAt = DateTime.UtcNow
                //        };
                //        await _walletTransactionRepository.AddAsync(transaction);
                //    }
                //}

                // Transition room status to Locked
                room.Status = RoomStatus.Locked;
                _matchRoomRepository.Update(room);

                // Cancel the auto-close job since the room is now fully paid and locked
                _roomAutoCloseService.CancelAutoClose(room.AutoCloseJobId);

                // Save all changes atomically
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Booking {BookingId} confirmed successfully for room {RoomId}. Room status changed to Locked.",
                    booking.BookingId, request.RoomId);

                // Schedule match start via Hangfire (will auto-transition to InProgress at match time)
                var matchStartTime = room.MatchDate.Add(room.StartTime);
                _matchLifecycleService.ScheduleMatchStart(request.RoomId, matchStartTime);

                _logger.LogInformation("Scheduled match start for room {RoomId} at {MatchStartTime}",
                    request.RoomId, matchStartTime);

                // Notify all participants about payment
                await _matchRoomHubService.NotifyParticipantPaidAsync(
                    request.RoomId,
                    currentUserId,
                    currentUser.FullName ?? "Unknown",
                    depositAmount,
                    room.TotalDepositCollected,
                    cancellationToken);

                // Notify all participants that booking is confirmed
                await _matchRoomHubService.NotifyBookingCreatedAsync(
                    request.RoomId,
                    booking.BookingId,
                    booking.BookingDate,
                    booking.StartTime,
                    booking.EndTime,
                    cancellationToken);

                await _matchRoomHubService.NotifyRoomStatusChangedAsync(
                    request.RoomId,
                    room.Status.ToString(),
                    cancellationToken);

                return Result.Success(new ProcessPaymentResponse(
                    true,
                    "Payment processed successfully. Booking confirmed.",
                    booking.BookingId,
                    booking.BookingDate,
                    booking.StartTime,
                    booking.EndTime
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for room {RoomId}", request.RoomId);
                throw;
            }
        }
        else
        {
            // Not all paid yet, just save current participant's payment
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} paid for room {RoomId}. Waiting for other participants...",
                currentUserId, request.RoomId);

            // Notify all participants about this payment
            await _matchRoomHubService.NotifyParticipantPaidAsync(
                request.RoomId,
                currentUserId,
                currentUser.FullName ?? "Unknown",
                depositAmount,
                room.TotalDepositCollected,
                cancellationToken);

            return Result.Success(new ProcessPaymentResponse(
                false,
                "Payment recorded. Waiting for other participants to pay.",
                null,
                null,
                null,
                null
            ));
        }
    }
}
