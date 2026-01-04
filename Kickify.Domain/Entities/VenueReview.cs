namespace Kickify.Domain.Entities;

public class VenueReview
{
    public Guid ReviewId { get; set; }
    public Guid VenueId { get; set; }
    public Guid UserId { get; set; }
    public Guid BookingId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
    public User User { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}
