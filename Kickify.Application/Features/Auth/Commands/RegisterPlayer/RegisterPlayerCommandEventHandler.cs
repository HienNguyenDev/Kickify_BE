using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Auth.Commands.RegisterPlayer;

public class RegisterPlayerCommandEventHandler : INotificationHandler<RegisterPlayerDomainEvent>
{
    private readonly IEmailJobService _emailJobService;

    public RegisterPlayerCommandEventHandler(IEmailJobService emailJobService)
    {
        _emailJobService = emailJobService;
    }

    public Task Handle(RegisterPlayerDomainEvent notification, CancellationToken cancellationToken)
    {
        _emailJobService.EnqueueSendOtpEmail(notification.Email, notification.OtpCode);
        return Task.CompletedTask;
    }
}
