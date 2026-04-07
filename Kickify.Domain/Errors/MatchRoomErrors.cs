using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class MatchRoomErrors
    {
        public static Error NotFound(Guid roomId) => Error.NotFound(
            "MatchRoom.NotFound",
            $"Room with ID {roomId} not found");

        public static readonly Error NotOpen = Error.Conflict(
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

        public static readonly Error InvalidTeamAssignment = Error.Conflict(
            "MatchRoom.InvalidTeam",
            "Invalid team assignment");

        public static Error InvalidTeam(string teamAssignment) => Error.Conflict(
            "MatchRoom.InvalidTeam",
            $"Invalid team assignment: {teamAssignment}");

        public static Error OutsideOperatingHours(TimeSpan startTime, TimeSpan endTime, TimeSpan openTime, TimeSpan closeTime) => Error.Conflict(
            "MatchRoom.OutsideOperatingHours",
            $"Requested time ({startTime:hh\\:mm} - {endTime:hh\\:mm}) is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})");

        public static Error SlotAlreadyBooked(TimeSpan startTime, TimeSpan endTime) => Error.Conflict(
            "MatchRoom.SlotAlreadyBooked",
            $"The time slot from {startTime:hh\\:mm} to {endTime:hh\\:mm} is already booked");

        public static Error VenueClosed(DateTime date) => Error.Conflict(
            "MatchRoom.VenueClosed",
            $"The venue is closed on {date:dddd}");

        public static Error InvalidFormat(string matchFormat) => Error.Conflict(
            "MatchRoom.InvalidFormat",
            $"Invalid match format: {matchFormat}");

        public static Error InvalidVisibility(string? visibility) => Error.Conflict(
            "MatchRoom.InvalidVisibility",
            $"Invalid visibility: {visibility}. Visibility must be Public or Private");

        public static readonly Error ConcurrencyConflict = Error.Conflict(
            "MatchRoom.ConcurrencyConflict",
            "Room was modified by another user. Please refresh and try again");

        public static readonly Error CreateFailed = Error.Conflict(
            "MatchRoom.CreateFailed",
            "Failed to create match room");

        public static readonly Error UpdateFailed = Error.Conflict(
            "MatchRoom.UpdateFailed",
            "Failed to update participant");

        public static readonly Error LeaveFailed = Error.Conflict(
            "MatchRoom.LeaveFailed",
            "Failed to leave room");

        // Kick Player Errors
        public static readonly Error OnlyHostCanKick = Error.Failure(
            "MatchRoom.OnlyHostCanKick",
            "Only the host can kick players from the room");

        public static readonly Error CannotKickSelf = Error.Conflict(
            "MatchRoom.CannotKickSelf",
            "Host cannot kick themselves. Use 'Leave Room' or 'Cancel Room' instead");

        public static readonly Error RoomNotActive = Error.Conflict(
            "MatchRoom.RoomNotActive",
            "Cannot kick players from a completed or cancelled room");

        public static Error PlayerNotInRoom(Guid userId) => Error.NotFound(
            "MatchRoom.PlayerNotInRoom",
            $"Player with ID {userId} is not in this room");

        public static readonly Error KickFailed = Error.Conflict(
            "MatchRoom.KickFailed",
            "Failed to kick player from room");

        // Private Room Errors
        public static readonly Error PasswordRequiredForPrivateRoom = Error.Conflict(
            "MatchRoom.PasswordRequired",
            "Password is required for private rooms");

        public static readonly Error IncorrectRoomPassword = Error.Conflict(
            "MatchRoom.IncorrectPassword",
            "Incorrect password for private room");

        public static readonly Error OnlyHostCanUpdatePrivacy = Error.Failure(
            "MatchRoom.OnlyHostCanUpdatePrivacy",
            "Only the host can update room privacy settings");

        public static readonly Error OnlyHostCanUpdateRoomInfo = Error.Failure(
            "MatchRoom.OnlyHostCanUpdateRoomInfo",
            "Only the host can update room name, description, and rules");

        // Formation Errors
        public static readonly Error NotCaptain = Error.Failure(
            "MatchRoom.NotCaptain",
            "Only the team captain can assign formations");

        public static Error InvalidFormation(string formationName, string matchFormat) => Error.Conflict(
            "MatchRoom.InvalidFormation",
            $"Formation '{formationName}' is not valid for {matchFormat}");

        public static Error InvalidSlotId(string slotId) => Error.Conflict(
            "MatchRoom.InvalidSlotId",
            $"Slot ID '{slotId}' is not valid for the selected formation");

        public static Error PlayerNotOnTeam(Guid playerId) => Error.Conflict(
            "MatchRoom.PlayerNotOnTeam",
            $"Player with ID {playerId} is not on your team");

        public static Error DuplicateSlotAssignment(string slotId) => Error.Conflict(
            "MatchRoom.DuplicateSlotAssignment",
            $"Slot '{slotId}' has already been assigned to another player");

        public static Error DuplicatePlayerAssignment(Guid playerId) => Error.Conflict(
            "MatchRoom.DuplicatePlayerAssignment",
            $"Player with ID {playerId} has already been assigned to another slot");

        public static readonly Error FormationUpdateFailed = Error.Conflict(
            "MatchRoom.FormationUpdateFailed",
            "Failed to update formation");

        public static readonly Error InvalidTeamForFormation = Error.Conflict(
            "MatchRoom.InvalidTeamForFormation",
            "Team must be either A or B to set formation");

        // Team Name Errors
        public static readonly Error TeamNameTooLong = Error.Conflict(
            "MatchRoom.TeamNameTooLong",
            "Team name must not exceed 50 characters");

        public static readonly Error TeamNameUpdateFailed = Error.Conflict(
            "MatchRoom.TeamNameUpdateFailed",
            "Failed to update team name");

        public static readonly Error CannotUpdateOtherTeamName = Error.Failure(
            "MatchRoom.CannotUpdateOtherTeamName",
            "You can only update the name of your own team");

        // Check-in Errors
        public static readonly Error AlreadyCheckedIn = Error.Conflict(
            "MatchRoom.AlreadyCheckedIn",
            "You have already checked in");

        public static readonly Error CheckInNotAllowed = Error.Conflict(
            "MatchRoom.CheckInNotAllowed",
            "Check-in is only allowed for Open or Locked rooms");

        public static readonly Error CheckInTooEarly = Error.Conflict(
            "MatchRoom.CheckInTooEarly",
            "Check-in is only allowed within 30 minutes before match start time");

        // Room Invitation Errors
        public static readonly Error CannotInviteSelf = Error.Conflict(
            "MatchRoom.CannotInviteSelf",
            "You cannot invite yourself to the room");

        public static readonly Error NotFriend = Error.Conflict(
            "MatchRoom.NotFriend",
            "You can only invite friends to the room");

        public static readonly Error InvitationAlreadySent = Error.Conflict(
            "MatchRoom.InvitationAlreadySent",
            "An invitation has already been sent to this user for this room");

        // Vote Errors
        public static readonly Error AlreadyVoted = Error.Conflict(
            "MatchRoom.AlreadyVoted",
            "You have already voted for this match result");

        public static readonly Error VoteNotAllowed = Error.Conflict(
            "MatchRoom.VoteNotAllowed",
            "Voting is only allowed during the reviewing phase");

        public static readonly Error AlreadySubmittedAfkVote = Error.Conflict(
            "MatchRoom.AlreadySubmittedAfkVote",
            "You have already submitted AFK votes for this match");

        public static readonly Error CannotVoteSelfAfk = Error.Conflict(
            "MatchRoom.CannotVoteSelfAfk",
            "You cannot vote yourself as AFK");

        public static readonly Error CannotVoteOpposingTeamAfk = Error.Conflict(
            "MatchRoom.CannotVoteOpposingTeamAfk",
            "Cannot vote players from opposing team");

        public static readonly Error NotInReviewingPhase = Error.Conflict(
            "MatchRoom.NotInReviewingPhase",
            "Match is not in reviewing phase");
        public static Error VotingPeriodClosed => Error.Conflict(
            "MatchRoom.VotingPeriodClosed",
            "Vote time has ended");

        public static readonly Error VenueSuspended = Error.Conflict(
            "MatchRoom.VenueSuspended",
            "Cannot create a match room because the venue is currently suspended");
        public static readonly Error VenueArchived = Error.Problem(
            "MatchRoom.VenueArchived",
            "Cannot create a match room because the venue is currently archived");

        public static Error TimeConflict(string? roomName) => Error.Conflict(
             "MatchRoom.TimeConflict",
             $"You cannot join because you already have a scheduled match in '{roomName ?? "another room"}' that conflicts with this time slot.");

        public static readonly Error OnlyHostCanCancel = Error.Conflict(
            "MatchRoom.OnlyHostCanCancel",
            "Only the room host can cancel the match room manually.");

        public static readonly Error CannotCancelWithin4Hours = Error.Conflict(
            "MatchRoom.CannotCancelWithin4Hours",
            "Match rooms cannot be cancelled within 4 hours of the start time.");

        public static readonly Error InvalidStateForCancel = Error.Conflict(
            "MatchRoom.InvalidStateForCancel",
            "Match room is not in a valid state to be cancelled. Only when it Open or Locked");

        public static readonly Error LeaveNotAllowed = Error.Conflict(
            "MatchRoom.LeaveNotAllowed",
            "You cannot leave the room while the lineup is LOCKED or a match is in progress. Absence will result in losing your DEPOSIT and affecting your REPUTATION points.");

    }
}
