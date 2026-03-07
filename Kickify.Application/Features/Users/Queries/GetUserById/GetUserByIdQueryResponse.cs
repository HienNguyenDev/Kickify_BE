using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public UserRole Role { get; set; }
        public string? PreferredPositions { get; set; }
        public int? ShirtNumber { get; set; }
        public string? PreferredFoot { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // PlayerProfile information
        public PlayerProfileDto? PlayerProfile { get; set; }

        // Achievements
        public List<AchievementDto> Achievements { get; set; } = new();
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
    }

    public class AchievementDto
    {
        public Guid AchievementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BadgeIconUrl { get; set; }
        public DateTime EarnedAt { get; set; }
    }
}
