using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerProfiles.Commands.CreatePlayerProfile
{
    public class CreatePlayerProfileCommand : ICommand<CreatePlayerProfileCommandResponse>
    {
        public Guid UserId { get; set; }
        public int? CurrentElo { get; set; }
        public decimal? TrustScore { get; set; }
        public string? PreferredPositions { get; set; }
    }
}
