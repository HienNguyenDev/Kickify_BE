namespace Kickify.Application.Features.Users.Commands.UploadUserAvatar;

public class UploadUserAvatarCommandResponse
{
    public Guid UserId { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
}
