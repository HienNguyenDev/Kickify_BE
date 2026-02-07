using Kickify.Api.Extensions;
using Kickify.Api.Infrastructure;
using Kickify.Api.Requests;
using Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom;
using Kickify.Application.Features.MatchRooms.Commands.JoinRoom;
using Kickify.Application.Features.MatchRooms.Commands.KickPlayer;
using Kickify.Application.Features.MatchRooms.Commands.LeaveRoom;
using Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant;
using Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms;
using Kickify.Application.Features.MatchRooms.Queries.GetMyMatchRooms;
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
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMatchRoomsQuery(date, matchFormat, availableOnly, page, pageSize);

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
    }
}
