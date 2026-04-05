namespace Kickify.Api.Requests;

public class RequestTransferHostRequest
{
    public Guid TargetUserId { get; set; }
}

public class RespondTransferHostRequest
{
    public bool IsAccepted { get; set; }
}
