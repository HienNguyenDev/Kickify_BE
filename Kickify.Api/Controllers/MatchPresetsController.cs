using Kickify.Api.Extensions;
using Kickify.Api.Infrastructure;
using Kickify.Api.Requests;
using Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset;
using Kickify.Application.Features.MatchPresets.Commands.DeleteMatchPreset;
using Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset;
using Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById;
using Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [ApiController]
    [Route("api/match-presets")]
    public class MatchPresetsController : ControllerBase
    {
        private readonly ISender _sender;

        public MatchPresetsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Create a new match preset
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IResult> CreatePreset([FromBody] CreateMatchPresetRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateMatchPresetCommand(
                request.FieldId,
                request.RoomName,
                request.MatchFormat,
                request.Visibility,
                request.StartTime,
                request.DurationMinutes,
                request.Rules,
                request.Password,
                request.Description
            );

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/match-presets/{result.Value.PresetId}", ApiResult<CreateMatchPresetResponse>.Success(result.Value));
            }

            return CustomResults.Problem(result);
        }

        /// <summary>
        /// Get my match presets (user's own presets)
        /// </summary>
        [HttpGet("mine")]
        [Authorize]
        public async Task<IResult> GetMyPresets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMyMatchPresetsQuery(page, pageSize);

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Get match preset by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IResult> GetPresetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetMatchPresetByIdQuery(id);

            var result = await _sender.Send(query, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Update a match preset (owner only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IResult> UpdatePreset(
            Guid id,
            [FromBody] UpdateMatchPresetRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateMatchPresetCommand(
                id,
                request.FieldId,
                request.RoomName,
                request.MatchFormat,
                request.Visibility,
                request.StartTime,
                request.DurationMinutes,
                request.Rules,
                request.Password,
                request.Description
            );

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }

        /// <summary>
        /// Delete a match preset (owner only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IResult> DeletePreset(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteMatchPresetCommand(id);

            var result = await _sender.Send(command, cancellationToken);

            return result.MatchOk();
        }
    }
}
