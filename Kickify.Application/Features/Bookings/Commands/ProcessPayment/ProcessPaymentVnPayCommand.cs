using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment;

/// <summary>
/// Variant of the check-in command used when the player has already paid via VNPay directly.
/// Skips wallet balance validation — money came from VNPay, not the wallet.
/// </summary>
public record ProcessPaymentVnPayCommand(
    Guid RoomId,
    Guid UserId,
    decimal Amount,
    string VnpayTransactionId
) : ICommand<ProcessPaymentResponse>;
