using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment
{
    public record ProcessPaymentCommand(
        Guid RoomId,
        Guid UserId
    ) : IRequest<Result<ProcessPaymentResponse>>;
}
