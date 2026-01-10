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
            var user = await _userRepository.GetUserWithDetailsAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                return Result.Failure<GetUserByIdQueryResponse>(UserErrors.NotFound(request.UserId));
            }

            // Check if user is active, return 404 if not
            if (!user.IsActive)
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
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PlayerProfile = user.PlayerProfile != null ? new PlayerProfileDto
                {
                    ProfileId = user.PlayerProfile.ProfileId,
                    CurrentElo = user.PlayerProfile.CurrentElo,
                    TrustScore = user.PlayerProfile.TrustScore,
                    TotalMatches = user.PlayerProfile.TotalMatches,
                    Wins = user.PlayerProfile.Wins,
                    Losses = user.PlayerProfile.Losses,
                    Draws = user.PlayerProfile.Draws,
                    MvpCount = user.PlayerProfile.MvpCount,
                    WinStreak = user.PlayerProfile.WinStreak,
                    MaxWinStreak = user.PlayerProfile.MaxWinStreak,
                    AfkCount = user.PlayerProfile.AfkCount,
                    ReportCount = user.PlayerProfile.ReportCount,
                    PreferredPositions = user.PlayerProfile.PreferredPositions
                } : null
            };

            return Result.Success(response);
        }
    }
}
