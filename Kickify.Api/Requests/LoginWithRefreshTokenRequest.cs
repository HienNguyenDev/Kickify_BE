namespace Kickify.Api.Requests
{
    public class LoginWithRefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
