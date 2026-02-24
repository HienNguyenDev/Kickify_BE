using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile;
using Kickify.Application.Features.PlayerProfiles.Commands.UpdatePlayerProfile;
using Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles;
using Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerProfileById;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [Route("api/playerprofiles")]
    [ApiController]
    public class PlayerProfilesController : ControllerBase
    {
        private readonly ISender _mediator;

        public PlayerProfilesController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all player profiles with pagination and filters
        /// </summary>
        [HttpGet]
        public async Task<IResult> GetAllPlayerProfiles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? minElo = null,
            [FromQuery] int? maxElo = null,
            [FromQuery] decimal? minTrustScore = null,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllPlayerProfilesQuery
            {
                Page = page,
                PageSize = pageSize,
                MinElo = minElo,
                MaxElo = maxElo,
                MinTrustScore = minTrustScore,
                SearchTerm = searchTerm
            };

            Result<GetAllPlayerProfilesQueryResponse> result = await _mediator.Send(query, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get player profile by ID
        /// </summary>
        [HttpGet("{profileId:guid}")]
        public async Task<IResult> GetPlayerProfileById(
            Guid profileId,
            CancellationToken cancellationToken)
        {
            var query = new GetPlayerProfileByIdQuery { ProfileId = profileId };
            Result<GetPlayerProfileByIdQueryResponse> result = await _mediator.Send(query, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Update player profile
        /// </summary>
        [HttpPut("{profileId:guid}")]
        public async Task<IResult> UpdatePlayerProfile(
            Guid profileId,
            [FromBody] UpdatePlayerProfileRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdatePlayerProfileCommand
            {
                ProfileId = profileId,
                CurrentElo = request.CurrentElo,
                TrustScore = request.TrustScore,
                TotalMatches = request.TotalMatches,
                Wins = request.Wins,
                Losses = request.Losses,
                Draws = request.Draws,
                MvpCount = request.MvpCount,
                WinStreak = request.WinStreak,
                MaxWinStreak = request.MaxWinStreak,
                AfkCount = request.AfkCount,
                ReportCount = request.ReportCount,
                PreferredPositions = request.PreferredPositions
            };

            Result<UpdatePlayerProfileCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Delete player profile (soft delete)
        /// </summary>
        [HttpDelete("{profileId:guid}")]
        public async Task<IResult> DeletePlayerProfile(
            Guid profileId,
            CancellationToken cancellationToken)
        {
            var command = new DeletePlayerProfileCommand { ProfileId = profileId };
            Result<DeletePlayerProfileCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }
    }
}
