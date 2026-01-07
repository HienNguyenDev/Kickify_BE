using Kickify.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kickify.Application.Features.Auth.Commands.RegisterPlayer
{
    public class RegisterPlayerCommand : ICommand<RegisterPlayerCommandResponse>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
