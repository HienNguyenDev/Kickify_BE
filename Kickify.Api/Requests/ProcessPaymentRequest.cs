namespace Kickify.Api.Requests
{
    public record ProcessPaymentRequest
    {
        public Guid RoomId { get; init; }
    }
}
