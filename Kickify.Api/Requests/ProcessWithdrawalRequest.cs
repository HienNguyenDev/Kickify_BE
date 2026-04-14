namespace Kickify.Api.Requests;

public class ProcessWithdrawalRequest
{
    public bool IsApproved { get; set; }
    public string? AdminNotes { get; set; }
}
