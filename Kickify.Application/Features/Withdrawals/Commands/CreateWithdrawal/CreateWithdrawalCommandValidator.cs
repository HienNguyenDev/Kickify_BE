using FluentValidation;

namespace Kickify.Application.Features.Withdrawals.Commands.CreateWithdrawal;

public class CreateWithdrawalCommandValidator : AbstractValidator<CreateWithdrawalCommand>
{
    public CreateWithdrawalCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(50000)
            .WithMessage("Minimum withdrawal amount is 50,000 VND");
    }
}
