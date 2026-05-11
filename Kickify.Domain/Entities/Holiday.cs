using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class Holiday : BaseEntity
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Venue> IgnoredByVenues { get; set; } = new List<Venue>();
}