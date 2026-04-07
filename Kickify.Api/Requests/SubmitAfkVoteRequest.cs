namespace Kickify.Api.Requests;

public class SubmitAfkVoteRequest
{
    /// <summary>
    /// Teammates considered AFK by the voter.
    /// </summary>
    public List<Guid> TargetPlayerIds { get; set; } = new();
}
