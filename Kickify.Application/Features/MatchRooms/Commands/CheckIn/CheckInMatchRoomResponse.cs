using System;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInMatchRoomResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? DistanceMeters { get; set; }
    public string CheckInMethod { get; set; } = string.Empty;
}
