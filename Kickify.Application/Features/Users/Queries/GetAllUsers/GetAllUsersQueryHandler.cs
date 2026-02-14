using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using System.Linq;

namespace Kickify.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, GetAllUsersQueryResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetAllUsersQueryResponse>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            var (users, total) = await _userRepository.GetPagedUsersAsync(
                role: request.Role,
                isActive: request.IsActive,
                searchTerm: request.SearchTerm,
                page: request.Page,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken
            );

            var userDtos = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                PlayerProfile = u.PlayerProfile != null ? new PlayerProfileDto
                {
                    ProfileId = u.PlayerProfile.ProfileId,
                    CurrentElo = u.PlayerProfile.CurrentElo,
                    TrustScore = u.PlayerProfile.TrustScore,
                    TotalMatches = u.PlayerProfile.TotalMatches,
                    Wins = u.PlayerProfile.Wins,
                    Losses = u.PlayerProfile.Losses,
                    Draws = u.PlayerProfile.Draws,
                    MvpCount = u.PlayerProfile.MvpCount,
                    WinStreak = u.PlayerProfile.WinStreak,
                    MaxWinStreak = u.PlayerProfile.MaxWinStreak,
                    ReportCount = u.PlayerProfile.ReportCount
                } : null
            }).ToList();

            var response = new GetAllUsersQueryResponse
            {
                Users = userDtos,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            };

            return Result.Success(response);
        }
    }
}
