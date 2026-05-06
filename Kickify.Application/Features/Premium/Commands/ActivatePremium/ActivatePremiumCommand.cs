using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Premium.Commands.ActivatePremium;

/// <summary>
/// Internal command dispatched by the VNPay IPN handler after a successful Premium payment.
/// Sets IsPremium = true and extends PremiumExpireAt by 30 days on the target user.
/// </summary>
public record ActivatePremiumCommand(Guid UserId) : ICommand<ActivatePremiumResponse>;

public record ActivatePremiumResponse(Guid UserId, bool IsPremium, DateTime PremiumExpireAt);
