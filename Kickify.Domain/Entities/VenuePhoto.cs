namespace Kickify.Domain.Entities;

public class VenuePhoto
{
    public Guid PhotoId { get; set; }
    public Guid VenueId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}
