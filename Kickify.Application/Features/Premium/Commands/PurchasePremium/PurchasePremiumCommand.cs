using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Premium.Commands.PurchasePremium;

/// <summary>
/// Initiates a Premium subscription purchase via VNPay.
/// Returns a VNPay payment URL the client should redirect the user to.
/// After successful IPN callback the user's IsPremium flag is set automatically.
/// </summary>
public record PurchasePremiumCommand : ICommand<PurchasePremiumResponse>;
