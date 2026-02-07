using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy
{
    public record UpdateRoomPrivacyCommand(
        Guid RoomId,
        string Visibility,
        string? Password
    ) : ICommand<UpdateRoomPrivacyResponse>;
}
