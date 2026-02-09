using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Auth.Commands.RegisterVenueOwner;

public class RegisterVenueOwnerCommandEventHandler : INotificationHandler<RegisterVenueOwnerDomainEvent>
{
    private readonly IEmailJobService _emailJobService;

    public RegisterVenueOwnerCommandEventHandler(IEmailJobService emailJobService)
    {
        _emailJobService = emailJobService;
    }

    public Task Handle(RegisterVenueOwnerDomainEvent notification, CancellationToken cancellationToken)
    {
        _emailJobService.EnqueueSendOtpEmail(notification.Email, notification.OtpCode);
        return Task.CompletedTask;
    }
}
