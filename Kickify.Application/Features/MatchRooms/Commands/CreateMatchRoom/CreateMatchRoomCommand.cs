using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public record CreateMatchRoomCommand(
        Guid UserId,
        Guid FieldId,
        DateTime MatchDate,
        TimeSpan StartTime,
        int DurationMinutes,
        string MatchFormat,
        string? Description,
        string? Rules,
        decimal? DepositPerPerson
    ) : IRequest<Result<CreateMatchRoomResponse>>;
}
