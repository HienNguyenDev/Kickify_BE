using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetUserByIdQueryResponse>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Use GetUserWithDetailsAsync to include PlayerProfile
            var user = await _userRepository.GetUserWithDetailsAsync(
                request.UserId,
                includeDeleted: true,
                cancellationToken: cancellationToken);

            if (user is null)
            {
                return Result.Failure<GetUserByIdQueryResponse>(UserErrors.NotFound(request.UserId));
            }

            var response = new GetUserByIdQueryResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role,
                PreferredPositions = user.PreferredPositions,
                ShirtNumber = user.ShirtNumber,
                PreferredFoot = user.PreferredFoot,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                BannedUntil = user.BannedUntil,
                DeletedAt = user.DeletedAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PlayerProfile = user.PlayerProfile != null ? new PlayerProfileDto
                {
                    ProfileId = user.PlayerProfile.ProfileId,
                    CurrentElo = user.PlayerProfile.CurrentElo,
                    CurrentRank = user.PlayerProfile.CurrentRank,
                    IsLegend = user.PlayerProfile.IsLegend,
                    TrustScore = user.PlayerProfile.TrustScore,
                    TotalMatches = user.PlayerProfile.TotalMatches,
                    Wins = user.PlayerProfile.Wins,
                    Losses = user.PlayerProfile.Losses,
                    Draws = user.PlayerProfile.Draws,
                    MvpCount = user.PlayerProfile.MvpCount,
                    WinStreak = user.PlayerProfile.WinStreak,
                    MaxWinStreak = user.PlayerProfile.MaxWinStreak,
                    ReportCount = user.PlayerProfile.ReportCount,
                } : null
            };

            // Map achievements
            response.Achievements = user.PlayerAchievements
                .Select(pa => new AchievementDto
                {
                    AchievementId = pa.AchievementId,
                    Name = pa.Achievement.Name,
                    Description = pa.Achievement.Description,
                    BadgeIconUrl = pa.Achievement.BadgeIconUrl,
                    EarnedAt = pa.EarnedAt
                })
                .OrderByDescending(a => a.EarnedAt)
                .ToList();

            return Result.Success(response);
        }
    }
}
