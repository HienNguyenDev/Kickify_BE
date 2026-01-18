namespace Kickify.Api.Requests
{
    public record UpdateParticipantRequest(
        string? TeamAssignment,
        string? Position
    );
}
