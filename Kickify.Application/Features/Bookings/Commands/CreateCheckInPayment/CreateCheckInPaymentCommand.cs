using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Commands.CreateCheckInPayment;

public record CreateCheckInPaymentCommand(Guid RoomId) : ICommand<CreateCheckInPaymentResponse>;
