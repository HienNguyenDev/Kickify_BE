using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public record UpdateParticipantCommand(
        Guid UserId,
        Guid RoomId,
        string? TeamAssignment,
        string? Position
    ) : IRequest<Result<UpdateParticipantResponse>>;
}
