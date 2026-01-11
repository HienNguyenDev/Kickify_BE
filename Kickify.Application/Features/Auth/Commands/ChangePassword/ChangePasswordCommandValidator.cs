using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.CurrentPassword).NotEmpty();

            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).Matches(@"(?=.*[A-Za-z])(?=.*\d)").NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
        }
    }
}
