using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid RoomId { get; set; }
    public Guid FieldId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal VenueAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public Field Field { get; set; } = null!;
    public ICollection<VenueReview> VenueReviews { get; set; } = new List<VenueReview>();
}
