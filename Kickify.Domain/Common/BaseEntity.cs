
namespace Kickify.Domain.Common;

public abstract class BaseEntity : IAuditableEntity, ISoftDeletable
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
