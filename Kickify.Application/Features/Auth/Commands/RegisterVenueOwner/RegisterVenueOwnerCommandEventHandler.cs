using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Event;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.RegisterVenueOwner
{
    public class RegisterVenueOwnerCommandEventHandler : INotificationHandler<RegisterVenueOwnerDomainEvent>
    {
        private readonly IMailService _mailService;
        private readonly IRedisOtpStore _otpStore;
        public RegisterVenueOwnerCommandEventHandler(IMailService mailService, IRedisOtpStore otpStore)
        {
            _mailService = mailService;
            _otpStore = otpStore;
        }

        public async Task Handle(RegisterVenueOwnerDomainEvent notification, CancellationToken cancellationToken)
        {
            await _otpStore.StoreAsync(notification.UserId, notification.OtpCode, TimeSpan.FromMinutes(5), cancellationToken);

            await _mailService.SendOtpAsync(notification.Email, notification.OtpCode);
        }
    }
}
