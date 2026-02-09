using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Users.Commands.UpdateFcmToken;

public class UpdateFcmTokenCommand : ICommand<UpdateFcmTokenCommandResponse>
{
    public string FcmToken { get; set; } = string.Empty;
}
