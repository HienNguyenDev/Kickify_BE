namespace Kickify.Api.Requests
{
    public record UpdateRoomPrivacyRequest(
        string Visibility,
        string? Password
    );
}
