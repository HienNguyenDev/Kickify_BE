using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public record UpdateParticipantCommand(
        Guid UserId,
        Guid RoomId,
        string? TeamAssignment,
        string? Position
    ) : ICommand<UpdateParticipantResponse>;
}
