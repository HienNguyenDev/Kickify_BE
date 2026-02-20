using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Application.Features.Achievements.Commands.CreateAchievement;

public class CreateAchievementCommand : ICommand<CreateAchievementResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CriteriaType { get; set; } = string.Empty;
    public int CriteriaValue { get; set; }
    public FileUploadRequest? IconFile { get; set; }
}
