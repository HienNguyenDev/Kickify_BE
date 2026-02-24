namespace Kickify.Application.Features.MatchRooms.Commands.RenameTeam
{
    public record RenameTeamResponse(
        Guid RoomId,
        string Team,
        string? TeamName,
        DateTime UpdatedAt
    );
}
