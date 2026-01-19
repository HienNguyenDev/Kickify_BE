namespace Kickify.Api.Requests
{
    public record class UpdateCommentRequest
    {
        public string Content { get; set; } = string.Empty;
    }

}
