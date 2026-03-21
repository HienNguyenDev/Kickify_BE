namespace Kickify.Api.Requests;

public class CreateVenueReviewRequest
{
    public Guid VenueId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
