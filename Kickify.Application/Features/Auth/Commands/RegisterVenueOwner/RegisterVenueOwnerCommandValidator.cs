using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.RegisterVenueOwner
{
    public class RegisterVenueOwnerCommandValidator : AbstractValidator<RegisterVenueOwnerCommand>
    {
        public RegisterVenueOwnerCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-=\[\]{};':""\\|,.<>\/])")
                .WithMessage("Password must be at least 8 characters long and include uppercase, lowercase, number, and special character");
        }
    }
}
