using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public record UpdateParticipantCommand(
        Guid RoomId,
        string? TeamAssignment,
        string? Position
    ) : ICommand<UpdateParticipantResponse>;
}
