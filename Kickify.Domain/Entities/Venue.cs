using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Venue : BaseEntity
{
    public Guid VenueId { get; set; }
    public Guid OwnerId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Description { get; set; }
    public string? Amenities { get; set; } // JSON: parking, shower, etc.
    public VenueStatus Status { get; set; } = VenueStatus.Draft;
    public string? AdminNotes { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<VenuePhoto> VenuePhotos { get; set; } = new List<VenuePhoto>();
    public ICollection<VenueOperatingHour> VenueOperatingHours { get; set; } = new List<VenueOperatingHour>();
    public ICollection<Field> Fields { get; set; } = new List<Field>();
    public ICollection<VenueReview> VenueReviews { get; set; } = new List<VenueReview>();
    public ICollection<VenueEvidence> VenueEvidences { get; set; } = new List<VenueEvidence>();
}
