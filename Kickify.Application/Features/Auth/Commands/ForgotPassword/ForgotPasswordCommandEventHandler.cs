using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandEventHandler : INotificationHandler<ForgotPasswordDomainEvent>
    {
        private readonly IMailService _mailService;
        public ForgotPasswordCommandEventHandler(IMailService mailService)
        {
            _mailService = mailService;
        }

        public async Task Handle(ForgotPasswordDomainEvent notification, CancellationToken cancellationToken)
        {
            await _mailService.SendResetPasswordAsync(notification.Email, notification.PasswordReset);
        }
    }
}
