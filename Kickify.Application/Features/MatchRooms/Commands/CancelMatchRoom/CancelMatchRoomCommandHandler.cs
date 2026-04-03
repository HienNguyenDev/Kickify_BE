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
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickify.Application.Features.MatchRooms.Commands.CancelMatchRoom;

public class CancelMatchRoomCommandHandler : ICommandHandler<CancelMatchRoomCommand, CancelMatchRoomResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private readonly IMatchRoomHubService _matchRoomHubService;
    private readonly ILogger<CancelMatchRoomCommandHandler> _logger;

    public CancelMatchRoomCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IBookingRepository bookingRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IFieldRepository fieldRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IMatchLifecycleService matchLifecycleService,
        IRoomAutoCloseService roomAutoCloseService,
        IMatchRoomHubService matchRoomHubService,
        ILogger<CancelMatchRoomCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _bookingRepository = bookingRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _fieldRepository = fieldRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _matchLifecycleService = matchLifecycleService;
        _roomAutoCloseService = roomAutoCloseService;
        _matchRoomHubService = matchRoomHubService;
        _logger = logger;
    }

    public async Task<Result<CancelMatchRoomResponse>> Handle(CancelMatchRoomCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room == null)
            return Result.Failure<CancelMatchRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));

        if (room.HostId != currentUserId)
            return Result.Failure<CancelMatchRoomResponse>(MatchRoomErrors.OnlyHostCanCancel);

        if (room.Status != RoomStatus.Open && room.Status != RoomStatus.Locked)
            return Result.Failure<CancelMatchRoomResponse>(MatchRoomErrors.InvalidStateForCancel);

        var matchStartTime = room.MatchDate.Date.Add(room.StartTime);
        var timeUntilMatch = matchStartTime - DateTime.UtcNow;

        if (timeUntilMatch.TotalHours < 4)
            return Result.Failure<CancelMatchRoomResponse>(MatchRoomErrors.CannotCancelWithin4Hours);

        bool isPenaltyZone = timeUntilMatch.TotalHours >= 4 && timeUntilMatch.TotalHours <= 24;
        decimal penaltyAmount = 0;

        Guid? venueOwnerId = null;
        if (room.FieldId.HasValue)
        {
            var field = await _fieldRepository.GetFieldWithVenueAsync(room.FieldId.Value, cancellationToken);
            venueOwnerId = field?.Venue?.OwnerId;
        }

        if (isPenaltyZone && room.TotalDepositCollected > 0 && venueOwnerId.HasValue)
        {
            penaltyAmount = room.TotalDepositCollected * 0.25m;
        }

        var hostWallet = await _walletRepository.GetByUserIdAsync(currentUserId, cancellationToken);
        var hostParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        decimal hostDeposit = (hostParticipant != null && hostParticipant.DepositPaid) ? (hostParticipant.DepositAmount ?? 0) : 0;

        if (isPenaltyZone && penaltyAmount > 0)
        {
            if (hostWallet == null || (hostWallet.Balance + hostDeposit) < penaltyAmount)
                return Result.Failure<CancelMatchRoomResponse>(WalletErrors.InsufficientBalanceForPenalty);
        }

        decimal totalRefunded = 0;
        var paidParticipants = room.RoomParticipants.Where(p => p.DepositPaid).ToList();
        
        foreach (var participant in paidParticipants)
        {
            var refundAmount = participant.DepositAmount ?? 0;
            if (refundAmount > 0)
            {
                var wallet = await _walletRepository.GetByUserIdAsync(participant.UserId, cancellationToken);
                if (wallet != null)
                {
                    wallet.Balance += refundAmount;
                    _walletRepository.Update(wallet);

                    var refundTx = new WalletTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        WalletId = wallet.WalletId,
                        TransactionType = TransactionType.Refund,
                        Amount = refundAmount,
                        BalanceAfter = wallet.Balance,
                        ReferenceId = room.RoomId,
                        Description = $"Refund (100%) for cancelled room {room.RoomName ?? room.RoomId.ToString()}",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _walletTransactionRepository.AddAsync(refundTx);
                    
                    totalRefunded += refundAmount;
                }
            }

            participant.DepositPaid = false;
            participant.CheckedIn = false;
            participant.CheckInTime = null;
            participant.DepositAmount = null;
            _roomParticipantRepository.Update(participant);
        }

        room.TotalDepositCollected = 0;

        if (isPenaltyZone && penaltyAmount > 0 && venueOwnerId.HasValue)
        {
            var updatedHostWallet = await _walletRepository.GetByUserIdAsync(currentUserId, cancellationToken);
            if (updatedHostWallet != null)
            {
                updatedHostWallet.Balance -= penaltyAmount;
                _walletRepository.Update(updatedHostWallet);

                var penaltyTx = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    WalletId = updatedHostWallet.WalletId,
                    TransactionType = TransactionType.Penalty,
                    Amount = -penaltyAmount,
                    BalanceAfter = updatedHostWallet.Balance,
                    ReferenceId = room.RoomId,
                    Description = $"Penalty fee (25%) for cancellation 4-24h prior. Room: {room.RoomName}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddAsync(penaltyTx);
            }

            var ownerWallet = await _walletRepository.GetByUserIdAsync(venueOwnerId.Value, cancellationToken);
            if (ownerWallet != null)
            {
                ownerWallet.Balance += penaltyAmount;
                _walletRepository.Update(ownerWallet);

                var compensationTx = new WalletTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    WalletId = ownerWallet.WalletId,
                    TransactionType = TransactionType.Compensation,
                    Amount = penaltyAmount,
                    BalanceAfter = ownerWallet.Balance,
                    ReferenceId = room.RoomId,
                    Description = $"Host Cancel Penalty Compensation for Room: {room.RoomName}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddAsync(compensationTx);
            }
        }

        room.Status = RoomStatus.Cancelled;
        _matchRoomRepository.Update(room);

        var booking = await _bookingRepository.GetBookingByRoomAsync(room.RoomId, cancellationToken);
        if (booking != null && booking.Status != BookingStatus.Cancelled)
        {
            booking.Status = BookingStatus.Cancelled;
            _bookingRepository.Update(booking);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _roomAutoCloseService.CancelAutoClose(room.AutoCloseJobId);
        _matchLifecycleService.CancelAllJobs(room.StartMatchJobId, room.EndMatchJobId, room.FinalizeResultJobId);

        await _matchRoomHubService.NotifyRoomCancelledAsync(room.RoomId, request.Reason, cancellationToken);

        return Result.Success(new CancelMatchRoomResponse(
            room.RoomId,
            request.Reason,
            totalRefunded,
            penaltyAmount,
            RoomStatus.Cancelled.ToString()
        ));
    }
}
