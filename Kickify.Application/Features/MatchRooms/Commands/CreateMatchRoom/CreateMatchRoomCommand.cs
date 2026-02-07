using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public record CreateMatchRoomCommand(
        Guid FieldId,
        DateTime MatchDate,
        TimeSpan StartTime,
        int DurationMinutes,
        string MatchFormat,
        string? RoomName,
        string? Description,
        string? Rules,
        string? Visibility,
        string? Password
    ) : ICommand<CreateMatchRoomResponse>;
}
