using FluentValidation;

namespace Kickify.Application.Features.Wallets.Commands.UpdateBankInfo;

public class UpdateBankInfoCommandValidator : AbstractValidator<UpdateBankInfoCommand>
{
    public UpdateBankInfoCommandValidator()
    {
        RuleFor(x => x.BankAccountNumber)
            .NotEmpty().WithMessage("Bank account number is required")
            .MaximumLength(50).WithMessage("Bank account number must not exceed 50 characters");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required")
            .MaximumLength(100).WithMessage("Bank name must not exceed 100 characters");

        RuleFor(x => x.AccountHolderName)
            .NotEmpty().WithMessage("Account holder name is required")
            .MaximumLength(100).WithMessage("Account holder name must not exceed 100 characters");
    }
}
