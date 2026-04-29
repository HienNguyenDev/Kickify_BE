using Kickify.Application.Abstractions.Messaging;
using System;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInMatchRoomGpsCommand : ICommand<CheckInMatchRoomResponse>
{
    public Guid RoomId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
