using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Chat.Commands.SendRoomMessage;

public class SendRoomMessageCommandHandler : ICommandHandler<SendRoomMessageCommand, SendRoomMessageCommandResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public SendRoomMessageCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<SendRoomMessageCommandResponse>> Handle(SendRoomMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = _userContext.UserId;

        // Get sender info
        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender is null)
        {
            return Result.Failure<SendRoomMessageCommandResponse>(UserErrors.NotFound(senderId));
        }

        // Check room exists
        var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
        if (room is null)
        {
            return Result.Failure<SendRoomMessageCommandResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        // Check if sender is participant in room
        var participant = await _roomParticipantRepository.GetParticipantByRoomAndUserAsync(request.RoomId, senderId, cancellationToken);
        if (participant is null)
        {
            return Result.Failure<SendRoomMessageCommandResponse>(ChatErrors.NotRoomParticipant);
        }

        // Check if user can send to this channel
        if (request.Channel == RoomChatChannel.TeamA && participant.TeamAssignment != TeamAssignment.A)
        {
            return Result.Failure<SendRoomMessageCommandResponse>(ChatErrors.CannotSendToTeamChannel);
        }
        if (request.Channel == RoomChatChannel.TeamB && participant.TeamAssignment != TeamAssignment.B)
        {
            return Result.Failure<SendRoomMessageCommandResponse>(ChatErrors.CannotSendToTeamChannel);
        }

        // Create message
        var message = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            RoomId = request.RoomId,
            SenderId = senderId,
            ConversationType = ConversationType.Room,
            RoomChatChannel = request.Channel,
            MessageText = request.MessageText,
            MessageType = MessageType.Text,
            SentAt = DateTime.UtcNow
        };

        await _chatMessageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new SendRoomMessageCommandResponse
        {
            MessageId = message.MessageId,
            RoomId = message.RoomId!.Value,
            Channel = message.RoomChatChannel!.Value,
            SenderId = message.SenderId,
            SenderFullName = sender.FullName ?? string.Empty,
            SenderAvatarUrl = sender.AvatarUrl,
            MessageText = message.MessageText,
            SentAt = message.SentAt
        };

        return Result.Success(response);
    }
}
