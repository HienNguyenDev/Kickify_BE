namespace Kickify.Api.Requests;

public record class UpdateAchievementRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CriteriaType { get; set; } = string.Empty;
    public int CriteriaValue { get; set; }
    public IFormFile? IconFile { get; set; }
}
