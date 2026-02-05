using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Chat.Queries.GetRoomMessages;

public class GetRoomMessagesQueryHandler : IQueryHandler<GetRoomMessagesQuery, GetRoomMessagesQueryResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IUserContext _userContext;

    public GetRoomMessagesQueryHandler(
        IChatMessageRepository chatMessageRepository,
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetRoomMessagesQueryResponse>> Handle(GetRoomMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Check room exists
        var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
        if (room is null)
        {
            return Result.Failure<GetRoomMessagesQueryResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        // Check if user is participant in room
        var participant = await _roomParticipantRepository.GetParticipantByRoomAndUserAsync(request.RoomId, userId, cancellationToken);
        if (participant is null)
        {
            return Result.Failure<GetRoomMessagesQueryResponse>(ChatErrors.NotRoomParticipant);
        }

        // Determine visibleFromDate based on channel
        DateTime? visibleFromDate = null;

        if (request.Channel == RoomChatChannel.General)
        {
            // General channel: Everyone can read all messages, no date restriction
            visibleFromDate = null;
        }
        else if (request.Channel == RoomChatChannel.TeamA)
        {
            // TeamA channel: Only TeamA members can access
            if (participant.TeamAssignment != TeamAssignment.A)
            {
                return Result.Failure<GetRoomMessagesQueryResponse>(ChatErrors.CannotAccessTeamChannel);
            }
            // Can only see messages from when they joined/switched to TeamA
            visibleFromDate = participant.UpdatedAt ?? participant.JoinDate;
        }
        else if (request.Channel == RoomChatChannel.TeamB)
        {
            // TeamB channel: Only TeamB members can access
            if (participant.TeamAssignment != TeamAssignment.B)
            {
                return Result.Failure<GetRoomMessagesQueryResponse>(ChatErrors.CannotAccessTeamChannel);
            }
            // Can only see messages from when they joined/switched to TeamB
            visibleFromDate = participant.UpdatedAt ?? participant.JoinDate;
        }

        // Get room participants for team info lookup
        var roomWithParticipants = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
        var participantsDict = roomWithParticipants?.RoomParticipants.ToDictionary(p => p.UserId, p => p.TeamAssignment) 
                               ?? new Dictionary<Guid, TeamAssignment>();

        // Get messages with visibleFromDate filter
        var (messages, total) = await _chatMessageRepository.GetRoomMessagesAsync(
            request.RoomId,
            request.Channel,
            visibleFromDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var messageDtos = messages.Select(m => new RoomMessageDto
        {
            MessageId = m.MessageId,
            SenderId = m.SenderId,
            SenderFullName = m.Sender?.FullName ?? string.Empty,
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            SenderTeam = participantsDict.TryGetValue(m.SenderId, out var team) ? team : null,
            MessageText = m.MessageText,
            SentAt = m.SentAt,
            IsEdited = m.IsEdited
        }).ToList();

        var response = new GetRoomMessagesQueryResponse
        {
            RoomId = request.RoomId,
            Channel = request.Channel,
            Messages = messageDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
