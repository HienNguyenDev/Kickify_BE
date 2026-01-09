using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.VerifyMail
{
    public class VerifyMailCommandValidator : AbstractValidator<VerifyMailCommand>
    {
        public VerifyMailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required.")
                .Length(6).WithMessage("OTP must be 6 characters long.");
        }
    }
}
