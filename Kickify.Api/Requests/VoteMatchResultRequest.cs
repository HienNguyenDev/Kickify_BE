using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class VoteMatchResultRequest
{
    /// <summary>
    /// Match result vote: TeamAWin, TeamBWin, or Draw
    /// </summary>
    public MatchResult Vote { get; set; }
}
