namespace Kickify.Api.Requests
{
    public record class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
}
