namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public record UpdateParticipantResponse(
        Guid ParticipantId,
        Guid RoomId,
        Guid UserId,
        string TeamAssignment,
        string? Position,
        DateTime UpdatedAt
    );
}
