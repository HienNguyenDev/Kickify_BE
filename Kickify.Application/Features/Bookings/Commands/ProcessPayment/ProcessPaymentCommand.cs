using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment
{
    public record ProcessPaymentCommand(
        Guid RoomId,
        Guid UserId
    ) : ICommand<ProcessPaymentResponse>;
}
