namespace Kickify.Api.Requests;

public class CreateVenueReviewRequest
{
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
