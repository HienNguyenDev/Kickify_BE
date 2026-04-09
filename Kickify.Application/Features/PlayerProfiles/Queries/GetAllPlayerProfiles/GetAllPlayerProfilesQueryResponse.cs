namespace Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles
{
    public class GetAllPlayerProfilesQueryResponse
    {
        public IEnumerable<PlayerProfileDto> Profiles { get; set; } = new List<PlayerProfileDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PlayerProfileDto
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserAvatarUrl { get; set; }
        public int CurrentElo { get; set; }
        public string CurrentRank { get; set; } = string.Empty;
        public bool IsLegend { get; set; }
        public decimal TrustScore { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int MvpCount { get; set; }
        public int WinStreak { get; set; }
        public int MaxWinStreak { get; set; }
        public string? PreferredPositions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
