namespace Kickify.Application.Features.MatchRooms.Commands.UpdateFormation
{
    public record UpdateFormationResponse(
        Guid RoomId,
        string Team,
        string FormationName,
        List<FormationSlotResponse> Assignments,
        DateTime UpdatedAt
    );

    public record FormationSlotResponse(
        Guid PlayerId,
        string PlayerName,
        string SlotId,
        string Position  // e.g., "Goalkeeper", "Defender", "Midfielder", "Forward"
    );
}
