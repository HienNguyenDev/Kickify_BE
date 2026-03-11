namespace Kickify.Domain.Entities;

public class VenueFeedback
{
    public Guid VenueFeedbackId { get; set; }
    public Guid VenueId { get; set; }
    public Guid SenderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    public Venue Venue { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
