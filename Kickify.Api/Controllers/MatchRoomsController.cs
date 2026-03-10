using Kickify.Api.Extensions;
using Kickify.Api.Infrastructure;
using Kickify.Api.Requests;
using Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom;
using Kickify.Application.Features.MatchRooms.Commands.GenerateRoomInviteLink;
using Kickify.Application.Features.MatchRooms.Commands.InviteFriendToRoom;
using Kickify.Application.Features.MatchRooms.Commands.JoinRoom;
using Kickify.Application.Features.MatchRooms.Commands.KickPlayer;
using Kickify.Application.Features.MatchRooms.Commands.LeaveRoom;
using Kickify.Application.Features.MatchRooms.Commands.RenameTeam;
using Kickify.Application.Features.MatchRooms.Commands.UpdateFormation;
using Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant;
using Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy;
using Kickify.Application.Features.MatchRooms.Commands.VoteMatchResult;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms;
using Kickify.Application.Features.MatchRooms.Queries.GetMyMatchRooms;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/match-rooms")]
    [Authorize]
    public class MatchRoomsController : ControllerBase
    {
        private readonly ISender _sender;

        public MatchRoomsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Create a new match room
        /// </summary>
        [HttpPost]
        public async Task<IResult> CreateRoom([FromBody] CreateMatchRoomRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateMatchRoomCommand(
                request.FieldId,
                request.MatchDate,
                request.StartTime,
                request.DurationMinutes,
                request.MatchFormat,
                request.RoomName,
                request.Description,
                request.Rules,
                request.Visibility,
                request.Password
            );

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/match-rooms/{result.Value.RoomId}", ApiResult<CreateMatchRoomResponse>.Success(result.Value));
            }

            return CustomResults.Problem(result);
        }

        /// <summary>
        /// Get room detail by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IResult> GetRoomById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetMatchRoomByIdQuery(id);

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get all match rooms for current user (as participant or host)
        /// </summary>
        [HttpGet("mine")]
        public async Task<IResult> GetMyRooms(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMyMatchRoomsQuery(page, pageSize);

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Search/Filter rooms
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetRooms(
            [FromQuery] DateTime? date,
            [FromQuery] string? matchFormat,
            [FromQuery] bool? availableOnly,
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] double? radiusKm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMatchRoomsQuery(date, matchFormat, availableOnly, latitude, longitude, radiusKm, page, pageSize);

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Join a room
        /// </summary>
        [HttpPost("{id}/join")]
        public async Task<IResult> JoinRoom(Guid id, [FromBody] JoinRoomRequest? request, CancellationToken cancellationToken)
        {
            var command = new JoinRoomCommand(id, request?.Password);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Leave a room
        /// </summary>
        [HttpPost("{id}/leave")]
        public async Task<IResult> LeaveRoom(Guid id, CancellationToken cancellationToken)
        {
            var command = new LeaveRoomCommand(id);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update participant's team and position
        /// </summary>
        [HttpPut("{id}/participants/me")]
        public async Task<IResult> UpdateParticipant(
            Guid id,
            [FromBody] UpdateParticipantRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateParticipantCommand(
                id,
                request.TeamAssignment,
                request.Position
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Kick a player from the room (Host only)
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="targetUserId">User ID to kick</param>
        [HttpDelete("{roomId}/participants/{targetUserId}")]
        public async Task<IResult> KickPlayer(
            Guid roomId,
            Guid targetUserId,
            CancellationToken cancellationToken)
        {
            var command = new KickPlayerCommand(
                roomId,
                targetUserId
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update room privacy settings (host only)
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <param name="request">Privacy settings</param>
        [HttpPatch("{id}/privacy")]
        public async Task<IResult> UpdateRoomPrivacy(
            Guid id,
            [FromBody] UpdateRoomPrivacyRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateRoomPrivacyCommand(
                id,
                request.Visibility,
                request.Password
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update team formation (Captain only)
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <param name="request">Formation settings with player slot assignments</param>
        /// <remarks>
        /// Only the captain of the specified team can update the formation.
        /// Valid formations depend on the match format:
        /// - 5vs5: "3-1", "2-2", "2-1-1", "1-2-1", "1-3"
        /// - 7vs7: "3-3", "4-2", "3-2-1", "3-1-2", "2-3-1", "4-1-1"
        /// - 11vs11: "4-4-2", "4-3-3", "3-5-2", "4-2-3-1", "5-3-2", "3-4-3", "4-5-1", "5-4-1"
        /// 
        /// Slot IDs format: GK-0 (Goalkeeper), DF-x (Defender), MF-x (Midfielder), FW-x (Forward)
        /// </remarks>
        [HttpPut("{id}/formation")]
        public async Task<IResult> UpdateFormation(
            Guid id,
            [FromBody] UpdateFormationRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateFormationCommand(
                id,
                request.Team,
                request.FormationName,
                request.Assignments.Select(a => new FormationSlotAssignment(a.PlayerId, a.SlotId)).ToList()
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update team name (Captain only)
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <param name="request">Team name settings</param>
        /// <remarks>
        /// Only the captain of the specified team can update the team name.
        /// Team name is optional and can be set to null to clear it.
        /// Maximum length: 50 characters.
        /// </remarks>
        [HttpPatch("{id}/team-name")]
        public async Task<IResult> RenameTeam(
            Guid id,
            [FromBody] RenameTeamRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RenameTeamCommand(
                id,
                request.Team,
                request.Name
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Vote for match result
        /// </summary>
        /// <remarks>
        /// Voting is only allowed during the Reviewing phase (after match ends).
        /// When 60% of participants have voted, the result will be finalized immediately.
        /// Otherwise, the result will be finalized after 12 hours based on majority vote.
        /// </remarks>
        [HttpPost("{id}/vote-result")]
        public async Task<IResult> VoteMatchResult(
            Guid id,
            [FromBody] VoteMatchResultRequest request,
            CancellationToken cancellationToken)
        {
            var command = new VoteMatchResultCommand(id, request.Vote);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Generate QR code and deep link for room invitation
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <remarks>
        /// Generates a deep link (kickify://room/{roomId}) and web link (https://kickify.app/room/{roomId})
        /// that can be encoded into a QR code for others to scan and join the room.
        /// Only participants of an Open room can generate invite links.
        /// </remarks>
        [HttpPost("{id}/invite-link")]
        public async Task<IResult> GenerateInviteLink(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new GenerateRoomInviteLinkCommand(id);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Invite a friend to join the room
        /// </summary>
        /// <param name="id">Room ID</param>
        /// <param name="request">Friend invitation request</param>
        /// <remarks>
        /// Invites a friend (must be already added as friend) to join the room.
        /// The invited friend will receive a push notification with a deep link to the room.
        /// A notification history record is also created.
        /// </remarks>
        [HttpPost("{id}/invite-friend")]
        public async Task<IResult> InviteFriend(
            Guid id,
            [FromBody] InviteFriendToRoomRequest request,
            CancellationToken cancellationToken)
        {
            var command = new InviteFriendToRoomCommand(id, request.FriendUserId);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
