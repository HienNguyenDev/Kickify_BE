using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.RegisterPlayer
{
    public class RegisterPlayerCommandValidator : AbstractValidator<RegisterPlayerCommand>
    {
        public RegisterPlayerCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).Matches(@"(?=.*[A-Za-z])(?=.*\d)");
        }
    }
}
