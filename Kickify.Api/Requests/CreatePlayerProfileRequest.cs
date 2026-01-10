namespace Kickify.Api.Requests
{
    public class CreatePlayerProfileRequest
    {
        public Guid UserId { get; set; }
        public int? CurrentElo { get; set; }
        public decimal? TrustScore { get; set; }
        public string? PreferredPositions { get; set; }
    }
}
