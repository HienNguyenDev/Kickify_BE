using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles
{
    public class GetAllPlayerProfilesQuery : IQuery<GetAllPlayerProfilesQueryResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? MinElo { get; set; }
        public int? MaxElo { get; set; }
        public decimal? MinTrustScore { get; set; }
        public string? SearchTerm { get; set; }
    }
}
