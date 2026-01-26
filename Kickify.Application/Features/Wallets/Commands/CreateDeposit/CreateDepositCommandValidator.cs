using FluentValidation;

namespace Kickify.Application.Features.Wallets.Commands.CreateDeposit;

public class CreateDepositCommandValidator : AbstractValidator<CreateDepositCommand>
{
    public CreateDepositCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(10000)
            .WithMessage("Minimum deposit is 10,000VND");
    }
}
