using System.Text.Json;

namespace Kickify.Api.Requests;

public class ReportPlayerRequest
{
    public Guid ReportedUserId { get; set; }
    public JsonElement ReportType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? MatchId { get; set; }
}
