namespace Kickify.Application.Features.Venues.Commands.SubmitVenueVerification;

public class SubmitVenueVerificationResponse
{
    public Guid VenueId { get; set; }
    public string Status { get; set; } = string.Empty;
}
