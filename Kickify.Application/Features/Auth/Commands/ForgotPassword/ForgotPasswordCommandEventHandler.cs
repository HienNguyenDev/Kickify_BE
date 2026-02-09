using Kickify.Application.Abstractions.Jobs;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandEventHandler : INotificationHandler<ForgotPasswordDomainEvent>
{
    private readonly IEmailJobService _emailJobService;

    public ForgotPasswordCommandEventHandler(IEmailJobService emailJobService)
    {
        _emailJobService = emailJobService;
    }

    public Task Handle(ForgotPasswordDomainEvent notification, CancellationToken cancellationToken)
    {
        _emailJobService.EnqueueSendResetPasswordEmail(notification.Email, notification.PasswordReset);
        return Task.CompletedTask;
    }
}
