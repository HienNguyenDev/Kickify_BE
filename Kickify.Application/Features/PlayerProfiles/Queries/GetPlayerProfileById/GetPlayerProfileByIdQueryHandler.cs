using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerProfileById
{
    public class GetPlayerProfileByIdQueryHandler : IQueryHandler<GetPlayerProfileByIdQuery, GetPlayerProfileByIdQueryResponse>
    {
        private readonly IPlayerProfileRepository _playerProfileRepository;

        public GetPlayerProfileByIdQueryHandler(IPlayerProfileRepository playerProfileRepository)
        {
            _playerProfileRepository = playerProfileRepository;
        }

        public async Task<Result<GetPlayerProfileByIdQueryResponse>> Handle(
            GetPlayerProfileByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Use FindAsync to get profile with filters, then get first result
            var profiles = await _playerProfileRepository
                .FindAsync(p => p.ProfileId == request.ProfileId);

            var profile = profiles.FirstOrDefault();

            if (profile is null)
            {
                return Result.Failure<GetPlayerProfileByIdQueryResponse>(
                    PlayerProfileErrors.NotFound(request.ProfileId));
            }

            var response = new GetPlayerProfileByIdQueryResponse
            {
                ProfileId = profile.ProfileId,
                UserId = profile.UserId,
                UserFullName = profile.User?.FullName,
                UserEmail = profile.User?.Email ?? string.Empty,
                UserAvatarUrl = profile.User?.AvatarUrl,
                CurrentElo = profile.CurrentElo,
                TrustScore = profile.TrustScore,
                TotalMatches = profile.TotalMatches,
                Wins = profile.Wins,
                Losses = profile.Losses,
                Draws = profile.Draws,
                MvpCount = profile.MvpCount,
                WinStreak = profile.WinStreak,
                MaxWinStreak = profile.MaxWinStreak,
                ReportCount = profile.ReportCount,
                PreferredPositions = profile.User?.PreferredPositions,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            return Result.Success(response);
        }
    }
}
