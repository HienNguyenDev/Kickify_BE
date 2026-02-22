namespace Kickify.Api.Requests;

public record UpdateVenueStatusRequest(
    string Status,
    string? AdminNotes
);
