using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryResponse
    {
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime? BannedUntil { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPremium { get; set; }
        public DateTime? PremiumExpireAt { get; set; }
        
        // PlayerProfile information
        public PlayerProfileDto? PlayerProfile { get; set; }
    }

    public class PlayerProfileDto
    {
        public Guid ProfileId { get; set; }
        public int CurrentElo { get; set; }
        public decimal TrustScore { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int MvpCount { get; set; }
        public int WinStreak { get; set; }
        public int MaxWinStreak { get; set; }
        public int AfkCount { get; set; }
        public int ReportCount { get; set; }
        public string? PreferredPositions { get; set; }
    }
}
