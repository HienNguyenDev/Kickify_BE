namespace Kickify.Api.Requests
{
    public record class ResendOtpRequest
    {
        public Guid UserId { get; init; }
    }
}
