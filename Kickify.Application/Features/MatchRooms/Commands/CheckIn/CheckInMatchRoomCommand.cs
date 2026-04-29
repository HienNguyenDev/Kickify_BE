using System;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInMatchRoomCommand : ICommand<CheckInMatchRoomResponse>
{
    public Guid RoomId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public FileUploadRequest? Photo { get; set; }
}
