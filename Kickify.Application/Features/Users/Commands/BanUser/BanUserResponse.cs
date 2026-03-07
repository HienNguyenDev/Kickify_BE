namespace Kickify.Application.Features.Users.Commands.BanUser;

public record BanUserResponse(
    Guid UserId,
    string Email,
    bool IsActive,
    DateTime? BannedUntil,
    string BanDuration,
    string Message);
