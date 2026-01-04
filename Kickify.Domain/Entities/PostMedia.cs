using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class PostMedia
{
    public Guid MediaId { get; set; }
    public Guid PostId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string? ThumbnailUrl { get; set; } // For videos
    public long? FileSize { get; set; } // In bytes
    public int? Duration { get; set; } // For videos (seconds)
    public int? Width { get; set; } // For images/videos
    public int? Height { get; set; } // For images/videos
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
}
