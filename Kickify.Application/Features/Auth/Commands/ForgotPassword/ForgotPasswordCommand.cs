using Kickify.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : ICommand<ForgotPasswordCommandResponse>
    {
        public string Email { get; set; } = string.Empty;
    }
}
