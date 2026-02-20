using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.InviteFriendToRoom;

public record InviteFriendToRoomCommand(
    Guid RoomId,
    Guid FriendUserId
) : ICommand<InviteFriendToRoomResponse>;
