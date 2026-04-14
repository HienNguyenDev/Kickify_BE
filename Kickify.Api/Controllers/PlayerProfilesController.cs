using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile;
using Kickify.Application.Features.PlayerProfiles.Commands.UpdatePlayerProfile;
using Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles;
using Kickify.Application.Features.PlayerProfiles.Queries.GetLeaderboard;
using Kickify.Application.Features.PlayerProfiles.Queries.GetMyEloBreakdown;
using Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;
using Kickify.Application.Features.PlayerProfiles.Queries.GetMyRank;
using Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerProfileById;
using Kickify.Application.Features.MatchFeedbacks.Queries.GetMyReceivedFeedbacks;
using Kickify.Application.Features.MatchFeedbacks.Queries.GetMyGivenFeedbacks;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        /// Get top 50 players leaderboard by ELO
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<IResult> GetLeaderboard(CancellationToken cancellationToken)
        {
            var query = new GetLeaderboardQuery();
            Result<GetLeaderboardResponse> result = await _mediator.Send(query, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get current user's rank in leaderboard
        /// </summary>
        [HttpGet("my-rank")]
        [Authorize]
        public async Task<IResult> GetMyRank(CancellationToken cancellationToken)
        {
            var query = new GetMyRankQuery();
            Result<GetMyRankResponse> result = await _mediator.Send(query, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get radar chart and AI assessments cached in database for current user.
        /// </summary>
        [HttpGet("me/radar-chart")]
        [Authorize]
        public async Task<IResult> GetMyRadarChart(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyRadarSnapshotQuery(), cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get ELO breakdown history for current user.
        /// </summary>
        [HttpGet("me/elo-breakdown")]
        [Authorize]
        public async Task<IResult> GetMyEloBreakdown([FromQuery] Guid? matchId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyEloBreakdownQuery(matchId), cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get feedbacks received by current user.
        /// </summary>
        [HttpGet("me/feedbacks/received")]
        [Authorize]
        public async Task<IResult> GetMyReceivedFeedbacks(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? rating,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMyReceivedFeedbacksQuery(fromDate, toDate, rating, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return result.MatchOk();
        }

        /// <summary>
        /// Get feedbacks given by current user.
        /// </summary>
        [HttpGet("me/feedbacks/given")]
        [Authorize]
        public async Task<IResult> GetMyGivenFeedbacks(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? rating,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMyGivenFeedbacksQuery(fromDate, toDate, rating, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
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
