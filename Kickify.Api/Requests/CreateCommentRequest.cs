namespace Kickify.Api.Requests
{
    public record class CreateCommentRequest
    {
        public Guid? ParentCommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
