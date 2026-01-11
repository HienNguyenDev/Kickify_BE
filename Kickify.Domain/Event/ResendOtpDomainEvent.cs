using Kickify.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Event
{
    public sealed record ResendOtpDomainEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public string OtpCode { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;


        public ResendOtpDomainEvent(Guid userId, string email, string otpCode)
        {
            UserId = userId;
            Email = email;
            OtpCode = otpCode;
        }
    }
}
