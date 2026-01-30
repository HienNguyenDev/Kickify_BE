using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class MatchRoomErrors
    {
        public static Error NotFound(Guid roomId) => Error.NotFound(
            "MatchRoom.NotFound",
            $"Room with ID {roomId} not found");

        public static readonly Error NotOpen = Error.Problem(
            "MatchRoom.NotOpen",
            "Room is not open for joining");

        public static readonly Error AlreadyJoined = Error.Conflict(
            "MatchRoom.AlreadyJoined",
            "User is already in this room");

        public static readonly Error RoomFull = Error.Conflict(
            "MatchRoom.RoomFull",
            "Room is already full");

        public static readonly Error NotParticipant = Error.NotFound(
            "MatchRoom.NotParticipant",
            "User is not a participant of this room");

        public static readonly Error InvalidTeamAssignment = Error.Problem(
            "MatchRoom.InvalidTeam",
            "Invalid team assignment");

        public static Error InvalidTeam(string teamAssignment) => Error.Problem(
            "MatchRoom.InvalidTeam",
            $"Invalid team assignment: {teamAssignment}");

        public static Error OutsideOperatingHours(TimeSpan startTime, TimeSpan endTime, TimeSpan openTime, TimeSpan closeTime) => Error.Problem(
            "MatchRoom.OutsideOperatingHours",
            $"Requested time ({startTime:hh\\:mm} - {endTime:hh\\:mm}) is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})");

        public static Error SlotAlreadyBooked(TimeSpan startTime, TimeSpan endTime) => Error.Conflict(
            "MatchRoom.SlotAlreadyBooked",
            $"The time slot from {startTime:hh\\:mm} to {endTime:hh\\:mm} is already booked");

        public static Error VenueClosed(DateTime date) => Error.Problem(
            "MatchRoom.VenueClosed",
            $"The venue is closed on {date:dddd}");

        public static Error InvalidFormat(string matchFormat) => Error.Problem(
            "MatchRoom.InvalidFormat",
            $"Invalid match format: {matchFormat}");

        public static readonly Error ConcurrencyConflict = Error.Conflict(
            "MatchRoom.ConcurrencyConflict",
            "Room was modified by another user. Please refresh and try again");

        public static readonly Error CreateFailed = Error.Problem(
            "MatchRoom.CreateFailed",
            "Failed to create match room");

        public static readonly Error UpdateFailed = Error.Problem(
            "MatchRoom.UpdateFailed",
            "Failed to update participant");

        public static readonly Error LeaveFailed = Error.Problem(
            "MatchRoom.LeaveFailed",
            "Failed to leave room");

        // Kick Player Errors
        public static readonly Error OnlyHostCanKick = Error.Failure(
            "MatchRoom.OnlyHostCanKick",
            "Only the host can kick players from the room");

        public static readonly Error CannotKickSelf = Error.Problem(
            "MatchRoom.CannotKickSelf",
            "Host cannot kick themselves. Use 'Leave Room' or 'Cancel Room' instead");

        public static readonly Error RoomNotActive = Error.Problem(
            "MatchRoom.RoomNotActive",
            "Cannot kick players from a completed or cancelled room");

        public static Error PlayerNotInRoom(Guid userId) => Error.NotFound(
            "MatchRoom.PlayerNotInRoom",
            $"Player with ID {userId} is not in this room");

        public static readonly Error KickFailed = Error.Problem(
            "MatchRoom.KickFailed",
            "Failed to kick player from room");
    }
}
