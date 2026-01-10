namespace Kickify.Application.Features.PlayerProfiles.Commands.CreatePlayerProfile
{
    public class CreatePlayerProfileCommandResponse
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }
        public int CurrentElo { get; set; }
        public decimal TrustScore { get; set; }
        public int TotalMatches { get; set; }
        public string? PreferredPositions { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
