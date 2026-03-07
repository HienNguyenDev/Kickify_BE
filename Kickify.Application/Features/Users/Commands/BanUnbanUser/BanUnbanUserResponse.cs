namespace Kickify.Application.Features.Users.Commands.BanUnbanUser;

public record BanUnbanUserResponse(
    Guid UserId,
    bool IsActive,
    string Message
);
