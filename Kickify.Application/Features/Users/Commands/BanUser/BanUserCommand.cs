using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Commands.BanUser;

public record BanUserCommand(
    Guid UserId,
    BanDuration Duration,
    string? Reason = null) : ICommand<BanUserResponse>;
