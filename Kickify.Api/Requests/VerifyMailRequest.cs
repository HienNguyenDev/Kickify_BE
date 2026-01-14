namespace Kickify.Api.Requests
{
    public class VerifyMailRequest
    {
        public Guid UserId { get; set; } 
        public string Otp { get; set; } = string.Empty;
    }
}
