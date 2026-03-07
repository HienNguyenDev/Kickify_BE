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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMatchRoomHubService _matchRoomHubService;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IBookingRepository bookingRepository,
        IFieldRepository fieldRepository,
        IVenueRepository venueRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUserRepository userRepository,
        IMatchRoomHubService matchRoomHubService,
        IMatchLifecycleService matchLifecycleService,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _bookingRepository = bookingRepository;
        _fieldRepository = fieldRepository;
        _venueRepository = venueRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _userRepository = userRepository;
        _matchRoomHubService = matchRoomHubService;
        _matchLifecycleService = matchLifecycleService;
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

            // Get field details
            var field = await _fieldRepository.GetFieldWithVenueAsync(room.FieldId!.Value, cancellationToken);
            if (field == null)
            {
                return Result.Failure<ProcessPaymentResponse>(FieldErrors.NotFound(room.FieldId));
            }

            // Pre-check availability at application level before attempting DB save
            var endTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes));
            var isSlotAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
                room.FieldId.Value, room.MatchDate, room.StartTime, endTime, cancellationToken);

            if (!isSlotAvailable)
            {
                _logger.LogWarning("Slot not available for room {RoomId}. Refunding current participant...", request.RoomId);

                // Refund current participant (their changes are still in-memory, not saved yet)
                // We DON'T save the current participant's payment — just refund previous participants
                await RefundAllPaidParticipantsAsync(request.RoomId, room.RoomName, cancellationToken);

                return Result.Failure<ProcessPaymentResponse>(BookingErrors.DoubleBooking);
            }

            // Calculate total amount
            var totalAmount = room.RoomParticipants.Sum(p => p.DepositAmount ?? 0);

            try
            {
                // Create booking
                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    FieldId = room.FieldId.Value,
                    RoomId = request.RoomId,
                    BookingDate = room.MatchDate,
                    StartTime = room.StartTime,
                    EndTime = endTime,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                await _bookingRepository.AddAsync(booking);

                // Transfer payment to venue owner's wallet
                var venue = await _venueRepository.GetByIdAsync(field.VenueId);
                if (venue != null)
                {
                    var wallet = await _walletRepository.GetByUserIdAsync(venue.OwnerId, cancellationToken);
                    if (wallet != null)
                    {
                        wallet.Balance += totalAmount;
                        _walletRepository.Update(wallet);

                        var transaction = new WalletTransaction
                        {
                            TransactionId = Guid.NewGuid(),
                            WalletId = wallet.WalletId,
                            TransactionType = TransactionType.BookingIncome,
                            Amount = totalAmount,
                            BalanceAfter = wallet.Balance,
                            ReferenceId = booking.BookingId,
                            Description = $"Booking income from room {room.RoomName ?? room.RoomId.ToString()}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _walletTransactionRepository.AddAsync(transaction);
                    }
                }

                // Transition room status to Locked
                room.Status = RoomStatus.Locked;
                _matchRoomRepository.Update(room);

                // Save all changes atomically — exclusion constraint checked here
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Booking {BookingId} created successfully for room {RoomId}. Room status changed to Locked.",
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

                return Result.Success(new ProcessPaymentResponse(
                    true,
                    "Payment processed successfully. Booking created.",
                    booking.BookingId,
                    booking.BookingDate,
                    booking.StartTime,
                    booking.EndTime
                ));
            }
            catch (DbUpdateException ex) when (IsExclusionConstraintViolation(ex))
            {
                // RACE CONDITION DETECTED: Another room booked this slot first
                // The current SaveChangesAsync failed → all changes in THIS transaction are rolled back
                // (current participant's payment, booking, venue wallet, room status)
                // BUT previous participants' payments were saved in earlier separate requests — need to refund them
                _logger.LogWarning(
                    "Race condition detected for room {RoomId}. Field {FieldId} already booked for {Date} {StartTime}-{EndTime}. Refunding all participants...",
                    request.RoomId, room.FieldId, room.MatchDate, room.StartTime, endTime);

                // Refund all participants who paid in previous requests using a NEW scope
                // (current DbContext is in a dirty state after failed SaveChanges)
                await RefundAllPaidParticipantsAsync(request.RoomId, room.RoomName, cancellationToken);

                return Result.Failure<ProcessPaymentResponse>(BookingErrors.DoubleBooking);
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

    /// <summary>
    /// Refund all participants who have already paid for this room.
    /// Uses a NEW service scope because the current DbContext may be dirty after a failed SaveChanges.
    /// </summary>
    private async Task RefundAllPaidParticipantsAsync(Guid roomId, string? roomName, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var participantRepo = scope.ServiceProvider.GetRequiredService<IRoomParticipantRepository>();
            var walletRepo = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
            var walletTransactionRepo = scope.ServiceProvider.GetRequiredService<IWalletTransactionRepository>();
            var matchRoomRepo = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Get all participants who have paid
            var participants = await participantRepo.GetParticipantsByRoomAsync(roomId, cancellationToken);
            var paidParticipants = participants.Where(p => p.DepositPaid).ToList();

            if (paidParticipants.Count == 0)
            {
                _logger.LogInformation("No paid participants to refund for room {RoomId}", roomId);
                return;
            }

            decimal totalRefunded = 0;

            foreach (var paidParticipant in paidParticipants)
            {
                var refundAmount = paidParticipant.DepositAmount ?? 0;
                if (refundAmount <= 0) continue;

                // Restore wallet balance
                var wallet = await walletRepo.GetByUserIdAsync(paidParticipant.UserId, cancellationToken);
                if (wallet != null)
                {
                    wallet.Balance += refundAmount;
                    walletRepo.Update(wallet);

                    // Create refund transaction record
                    var refundTransaction = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        WalletId = wallet.WalletId,
                        TransactionType = TransactionType.Refund,
                        Amount = refundAmount,
                        BalanceAfter = wallet.Balance,
                        ReferenceId = roomId,
                        Description = $"Refund for room {roomName ?? roomId.ToString()} - slot no longer available",
                        CreatedAt = DateTime.UtcNow
                    };
                    await walletTransactionRepo.AddAsync(refundTransaction);

                    totalRefunded += refundAmount;
                    _logger.LogInformation("Refunded {Amount} to user {UserId} for room {RoomId}",
                        refundAmount, paidParticipant.UserId, roomId);
                }

                // Reset participant payment status
                paidParticipant.DepositPaid = false;
                paidParticipant.CheckedIn = false;
                paidParticipant.CheckInTime = null;
                paidParticipant.DepositAmount = null;
                participantRepo.Update(paidParticipant);
            }

            // Reset room's total deposit collected and cancel the room
            var room = await matchRoomRepo.GetByIdAsync(roomId);
            if (room != null)
            {
                room.TotalDepositCollected = 0;
                room.Status = RoomStatus.Cancelled;
                matchRoomRepo.Update(room);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Refund completed for room {RoomId}. Total refunded: {TotalRefunded} to {Count} participants",
                roomId, totalRefunded, paidParticipants.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL: Failed to refund participants for room {RoomId}. Manual intervention required!", roomId);
            // Don't rethrow — the caller already returns a failure result.
            // This error should be monitored and resolved manually.
        }
    }

    private bool IsExclusionConstraintViolation(DbUpdateException ex)
    {
        // Check if the exception is caused by PostgreSQL exclusion constraint violation
        // Error code 23P01 = exclusion_violation
        var innerException = ex.InnerException;
        if (innerException != null)
        {
            var message = innerException.Message;
            return message.Contains("23P01") || message.Contains("no_overlap_booking") || message.Contains("exclusion_violation");
        }

        return false;
    }
}
