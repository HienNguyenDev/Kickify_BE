namespace Kickify.Api.Requests;

public class ProcessReportRequest
{
    public bool IsApproved { get; set; }
    public string? AdminNotes { get; set; }
    public string? ActionTaken { get; set; }
}
