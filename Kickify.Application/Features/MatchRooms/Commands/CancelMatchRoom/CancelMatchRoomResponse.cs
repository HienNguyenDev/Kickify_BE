using System;

namespace Kickify.Application.Features.MatchRooms.Commands.CancelMatchRoom;

public record CancelMatchRoomResponse(
    Guid RoomId,
    string Reason,
    decimal RefundedAmount,
    decimal PenaltyAmount,
    string Status
);
