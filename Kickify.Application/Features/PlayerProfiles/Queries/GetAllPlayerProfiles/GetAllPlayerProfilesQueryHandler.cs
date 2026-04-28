using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles
{
    public class GetAllPlayerProfilesQueryHandler : IQueryHandler<GetAllPlayerProfilesQuery, GetAllPlayerProfilesQueryResponse>
    {
        private readonly IPlayerProfileRepository _playerProfileRepository;

        public GetAllPlayerProfilesQueryHandler(IPlayerProfileRepository playerProfileRepository)
        {
            _playerProfileRepository = playerProfileRepository;
        }

        public async Task<Result<GetAllPlayerProfilesQueryResponse>> Handle(
            GetAllPlayerProfilesQuery request,
            CancellationToken cancellationToken)
        {
            var (profiles, total) = await _playerProfileRepository.GetPagedProfilesAsync(
                minElo: request.MinElo,
                maxElo: request.MaxElo,
                minTrustScore: request.MinTrustScore,
                searchTerm: request.SearchTerm,
                positions: request.Positions,
                preferredFoot: request.PreferredFoot,
                highFormOnly: request.HighFormOnly,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken
            );

            var profileDtos = profiles.Select(p => new PlayerProfileDto
            {
                ProfileId = p.ProfileId,
                UserId = p.UserId,
                UserFullName = p.User?.FullName,
                UserEmail = p.User?.Email ?? string.Empty,
                UserAvatarUrl = p.User?.AvatarUrl,
                CurrentElo = p.CurrentElo,
                CurrentRank = p.CurrentRank,
                IsLegend = p.IsLegend,
                TrustScore = p.TrustScore,
                TotalMatches = p.TotalMatches,
                Wins = p.Wins,
                Losses = p.Losses,
                Draws = p.Draws,
                MvpCount = p.MvpCount,
                WinStreak = p.WinStreak,
                MaxWinStreak = p.MaxWinStreak,
                PreferredPositions = p.User?.PreferredPositions,
                PreferredFoot = p.User?.PreferredFoot,
                CreatedAt = p.CreatedAt
            }).ToList();

            var response = new GetAllPlayerProfilesQueryResponse
            {
                Profiles = profileDtos,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            };

            return Result.Success(response);
        }
    }
}
