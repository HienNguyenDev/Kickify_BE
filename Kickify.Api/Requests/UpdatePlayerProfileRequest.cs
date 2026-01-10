namespace Kickify.Api.Requests
{
    public class UpdatePlayerProfileRequest
    {
        public int? CurrentElo { get; set; }
        public decimal? TrustScore { get; set; }
        public int? TotalMatches { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Draws { get; set; }
        public int? MvpCount { get; set; }
        public int? WinStreak { get; set; }
        public int? MaxWinStreak { get; set; }
        public int? AfkCount { get; set; }
        public int? ReportCount { get; set; }
        public string? PreferredPositions { get; set; }
    }
}
