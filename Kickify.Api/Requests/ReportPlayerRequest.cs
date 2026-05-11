namespace Kickify.Api.Requests;

public class ReportPlayerRequest
{
    public Guid ReportedUserId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? MatchId { get; set; }
}
