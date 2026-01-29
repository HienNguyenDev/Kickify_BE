using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.KickPlayer
{
    /// <summary>
    /// Command to kick a player from a match room (Host only)
    /// </summary>
    public record KickPlayerCommand(
        Guid HostId,           // Current user ID (must be host)
        Guid RoomId,           // Room to kick from
        Guid TargetUserId,     // User to kick
        string? Reason = null  // Optional reason for kicking
    ) : ICommand<KickPlayerResponse>;
}
