using Kickify.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.VerifyMail
{
    public class VerifyMailCommand : ICommand<VerifyMailCommandResponse>
    {
        public Guid UserId { get; set; }
        public string Otp { get; set; } = string.Empty;
    }
}
