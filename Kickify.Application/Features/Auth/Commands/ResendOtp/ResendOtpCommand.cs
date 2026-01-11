using Kickify.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ResendOtp
{
    public class ResendOtpCommand : ICommand<ResendOtpCommandResponse>
    {
        public Guid UserId { get; set; }
    }
}
