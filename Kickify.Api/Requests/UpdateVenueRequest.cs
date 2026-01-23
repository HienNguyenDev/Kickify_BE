namespace Kickify.Api.Requests;

public sealed record UpdateVenueRequest(
    string? Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    string? ContactPhone,
    string? ContactEmail,
    string? Description,
    string? Amenities
);
