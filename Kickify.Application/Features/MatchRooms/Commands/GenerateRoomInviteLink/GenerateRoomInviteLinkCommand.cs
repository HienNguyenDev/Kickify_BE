using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.GenerateRoomInviteLink;

public record GenerateRoomInviteLinkCommand(
    Guid RoomId
) : ICommand<GenerateRoomInviteLinkResponse>;
