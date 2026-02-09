using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.RenameTeam
{
    public record RenameTeamCommand(
        Guid RoomId,
        string Team,  // "A" or "B"
        string? Name  // Team name, nullable to allow clearing
    ) : ICommand<RenameTeamResponse>;
}
