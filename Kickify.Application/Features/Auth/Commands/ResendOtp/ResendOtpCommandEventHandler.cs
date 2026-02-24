using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommandEventHandler : INotificationHandler<ResendOtpDomainEvent>
{
    private readonly IEmailJobService _emailJobService;

    public ResendOtpCommandEventHandler(IEmailJobService emailJobService)
    {
        _emailJobService = emailJobService;
    }

    public Task Handle(ResendOtpDomainEvent notification, CancellationToken cancellationToken)
    {
        _emailJobService.EnqueueSendOtpEmail(notification.Email, notification.OtpCode);
        return Task.CompletedTask;
    }
}
