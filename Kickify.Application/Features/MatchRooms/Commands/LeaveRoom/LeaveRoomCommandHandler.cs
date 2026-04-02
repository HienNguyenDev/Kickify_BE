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
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public class LeaveRoomCommandHandler : ICommandHandler<LeaveRoomCommand, LeaveRoomResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomAutoCloseService _roomAutoCloseService;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IPublisher _publisher;
        private readonly ILogger<LeaveRoomCommandHandler> _logger;

        public LeaveRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IChatMessageRepository chatMessageRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IBookingRepository bookingRepository,
            IRoomAutoCloseService roomAutoCloseService,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IPublisher publisher,
            ILogger<LeaveRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _chatMessageRepository = chatMessageRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _bookingRepository = bookingRepository;
            _roomAutoCloseService = roomAutoCloseService;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Result<LeaveRoomResponse>> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<LeaveRoomResponse>(UserErrors.NotFound(userId));
            }

            // Get room with participants (WITH TRACKING for update/delete)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // ==========================================
            // RÀNG BUỘC 1: BUSINESS RULE (Chỉ cho phép Leave khi phòng Open)
            // ==========================================
            if (room.Status != RoomStatus.Open)
            {
                _logger.LogWarning("User {UserId} attempted to leave room {RoomId} but status is {Status}",
                    userId, request.RoomId, room.Status);

                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.LeaveNotAllowed);
            }

            // Find participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotParticipant);
            }

            try
            {
                // CAPTAIN LOGIC: Handle succession before removal
                if (participant.IsCaptain && participant.TeamAssignment != TeamAssignment.Unassigned)
                {
                    var newCaptainId = await _roomParticipantRepository.AssignNewCaptainAsync(
                        request.RoomId, participant.TeamAssignment, userId, cancellationToken);

                    if (newCaptainId.HasValue)
                    {
                        _logger.LogInformation("Captain succession: User {OldCaptain} leaving team {Team}, new captain is {NewCaptain}",
                            userId, participant.TeamAssignment, newCaptainId.Value);
                    }
                }

                // ==========================================
                // RÀNG BUỘC 2: XỬ LÝ TIỀN BẠC (REFUND 100%)
                // Gom chung lên đây để xử lý cho cả Host và Player
                // ==========================================
                if (participant.DepositPaid && participant.DepositAmount.HasValue && participant.DepositAmount.Value > 0)
                {
                    var refundAmount = participant.DepositAmount.Value;

                    // 1. Trừ tiền khỏi tổng của phòng
                    room.TotalDepositCollected -= refundAmount;

                    // 2. Cộng trả tiền vào ví Wallet
                    var wallet = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
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
                            Description = $"Refund (100%) for leaving room {room.RoomName ?? room.RoomId.ToString()}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _walletTransactionRepository.AddAsync(refundTx);
                    }

                    _logger.LogInformation("Refunded {RefundAmount} to User {UserId} leaving room {RoomId}. New room total: {TotalDeposit}",
                        refundAmount, userId, request.RoomId, room.TotalDepositCollected);
                }

                bool isHost = room.HostId == userId;
                bool isRoomDeleted = false;
                Guid? newHostId = null;
                string message;

                // ==========================================
                // RÀNG BUỘC 3: XỬ LÝ QUYỀN HOST & XÓA PHÒNG
                // ==========================================
                if (isHost && room.FilledSlots == 1)
                {
                    // Host is the only one left - delete the room
                    isRoomDeleted = true;
                    _matchRoomRepository.Remove(room);

                    // Xóa/Hủy báo thức Auto-Close của Hangfire
                    _roomAutoCloseService.CancelAutoClose(room.AutoCloseJobId);

                    // Xóa Pending Booking (nếu hệ thống bạn tạo Booking ngay lúc Open)
                    var booking = await _bookingRepository.GetBookingByRoomAsync(room.RoomId, cancellationToken);
                    if (booking != null && booking.Status != BookingStatus.Cancelled)
                    {
                        booking.Status = BookingStatus.Cancelled;
                        _bookingRepository.Update(booking);
                    }

                    message = "Room deleted as you were the last participant";
                    _logger.LogInformation("Room {RoomId} deleted as host {UserId} was the last participant", request.RoomId, userId);
                }
                else if (isHost && room.FilledSlots > 1)
                {
                    // Host leaves but others remain - reassign host
                    var newHost = room.RoomParticipants.FirstOrDefault(p => p.UserId != userId);
                    if (newHost != null)
                    {
                        room.HostId = newHost.UserId;
                        newHostId = newHost.UserId;
                        _logger.LogInformation("Room {RoomId} host reassigned from {OldHostId} to {NewHostId}",
                            request.RoomId, userId, newHost.UserId);
                    }

                    // Reset Privacy
                    if (room.Visibility == Visibility.Private)
                    {
                        room.Visibility = Visibility.Public;
                        room.RoomPassword = null;
                        _logger.LogInformation("Room {RoomId} privacy reset to Public", request.RoomId);

                        var systemMessage = new ChatMessage
                        {
                            MessageId = Guid.NewGuid(),
                            RoomId = room.RoomId,
                            SenderId = room.HostId,
                            ConversationType = ConversationType.Room,
                            RoomChatChannel = RoomChatChannel.General,
                            MessageText = "Room privacy has been reset to Public because the previous host left.",
                            MessageType = MessageType.System,
                            SentAt = DateTime.UtcNow
                        };
                        await _chatMessageRepository.AddAsync(systemMessage);

                        await _matchRoomHubService.NotifyRoomPrivacyUpdatedAsync(
                            room.RoomId, Visibility.Public.ToString(), false, cancellationToken);
                    }

                    // ==========================================
                    // RÀNG BUỘC 4: CẬP NHẬT DATA
                    // ==========================================
                    _roomParticipantRepository.Remove(participant);
                    room.FilledSlots--;
                    _matchRoomRepository.Update(room);
                    message = "You left the room. Host role transferred to another participant";
                }
                else
                {
                    // Regular participant leaves
                    _roomParticipantRepository.Remove(participant);
                    room.FilledSlots--;
                    _matchRoomRepository.Update(room);
                    message = "You left the room successfully";
                    _logger.LogInformation("User {UserId} left room {RoomId}. Filled: {FilledSlots}/{TotalSlots}",
                        userId, request.RoomId, room.FilledSlots, room.TotalSlots);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!isRoomDeleted)
                {
                    await _publisher.Publish(
                        new MatchRoomPlayerLeftHostNotifyDomainEvent(
                            room.RoomId,
                            room.HostId,
                            user.FullName ?? user.Email,
                            room.RoomName),
                        cancellationToken);
                }

                // Send real-time notification to all room participants
                await _matchRoomHubService.NotifyUserLeftAsync(
                    room.RoomId,
                    user.UserId,
                    user.FullName ?? user.Email,
                    room.FilledSlots,
                    room.TotalSlots,
                    isRoomDeleted,
                    newHostId,
                    cancellationToken);

                return Result.Success(new LeaveRoomResponse(
                    room.RoomId,
                    userId,
                    room.FilledSlots,
                    room.TotalSlots,
                    message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room");
                throw;
            }
        }
    }
}