namespace Kickify.Api.Requests
{
    public record ProcessPaymentRequest
    {
        public Guid RoomId { get; init; }
        public Guid UserId { get; init; }
    }
}
