using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ResendOtp
{
    public class ResendOtpCommandEventHandler : INotificationHandler<ResendOtpDomainEvent>
    {
        private readonly IMailService _mailService;
        private readonly IRedisOtpStore _otpStore;

        public ResendOtpCommandEventHandler(IMailService mailService, IRedisOtpStore otpStore)
        {
            _mailService = mailService;
            _otpStore = otpStore;
        }

        public async Task Handle(ResendOtpDomainEvent notification, CancellationToken cancellationToken)
        {
            await _otpStore.StoreAsync(notification.UserId, notification.OtpCode, TimeSpan.FromMinutes(5), cancellationToken);

            await _mailService.SendOtpAsync(notification.Email, notification.OtpCode);
        }
    }
}
