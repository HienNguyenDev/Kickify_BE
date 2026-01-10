namespace Kickify.Api.Requests;

public record class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public List<IFormFile>? Files { get; set; }
}
