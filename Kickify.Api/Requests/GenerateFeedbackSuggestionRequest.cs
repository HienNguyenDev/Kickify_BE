namespace Kickify.Api.Requests;

public class GenerateFeedbackSuggestionRequest
{
    public int StarRating { get; set; }
    public int Count { get; set; } = 3;
    public string? Role { get; set; }
}
