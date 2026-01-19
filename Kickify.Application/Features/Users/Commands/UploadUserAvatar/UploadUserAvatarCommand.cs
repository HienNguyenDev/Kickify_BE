using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Application.Features.Users.Commands.UploadUserAvatar;

public class UploadUserAvatarCommand : ICommand<UploadUserAvatarCommandResponse>
{
    public FileUploadRequest File { get; set; } = null!;
}
