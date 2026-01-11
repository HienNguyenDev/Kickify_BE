using Kickify.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Event
{
    public record ForgotPasswordDomainEvent(string Email, string PasswordReset) : IDomainEvent;
}
