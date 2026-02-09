using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateFormation
{
    public record UpdateFormationCommand(
        Guid RoomId,
        string Team,  // "A" or "B"
        string FormationName,  // e.g., "4-3-3"
        List<FormationSlotAssignment> Assignments
    ) : ICommand<UpdateFormationResponse>;

    public record FormationSlotAssignment(
        Guid PlayerId,
        string SlotId  // e.g., "GK-0", "DF-1", "MF-2", "FW-0"
    );
}
