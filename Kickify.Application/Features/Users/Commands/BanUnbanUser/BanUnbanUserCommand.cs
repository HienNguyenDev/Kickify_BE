using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Users.Commands.BanUnbanUser;

public record BanUnbanUserCommand(
    Guid UserId,
    bool Ban
) : ICommand<BanUnbanUserResponse>;
