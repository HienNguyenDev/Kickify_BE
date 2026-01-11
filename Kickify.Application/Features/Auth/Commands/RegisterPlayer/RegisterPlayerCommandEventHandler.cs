using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace Kickify.Application.Features.Auth.Commands.RegisterPlayer
{
    public class RegisterPlayerCommandEventHandler : INotificationHandler<RegisterPlayerDomainEvent>
    {
        private readonly IMailService _mailService;
        private readonly IRedisOtpStore _otpStore;

        public RegisterPlayerCommandEventHandler(IMailService mailService,IRedisOtpStore otpStore)
        {
            _mailService = mailService;
            _otpStore = otpStore;
        }

        public async Task Handle(RegisterPlayerDomainEvent notification, CancellationToken cancellationToken)
        {
            await _otpStore.StoreAsync(notification.UserId,notification.OtpCode,TimeSpan.FromMinutes(5),cancellationToken);

            await _mailService.SendOtpAsync(notification.Email, notification.OtpCode);
        }
    }
}
