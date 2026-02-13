using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public record CheckInCommand(Guid RoomId) : ICommand<CheckInResponse>;
