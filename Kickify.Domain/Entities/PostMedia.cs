using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class PostMedia : BaseEntity
{
    public Guid MediaId { get; set; }
    public Guid PostId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;     // MinIO object path
    public string PublicUrl { get; set; } = string.Empty;       // CDN URL
    public string ContentType { get; set; } = string.Empty;     // MIME type
    public string BucketName { get; set; } = "kickify-media";
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; } // In bytes
    public string? ThumbnailStoragePath { get; set; }
    public string? ThumbnailUrl { get; set; } // For videos
    public int? Duration { get; set; } // For videos (seconds)
    public int? Width { get; set; } // For images/videos
    public int? Height { get; set; } // For images/videos
    public int DisplayOrder { get; set; } = 0;
    public bool IsProcessed { get; set; } = true;

    // Navigation properties
    public Post Post { get; set; } = null!;
}
