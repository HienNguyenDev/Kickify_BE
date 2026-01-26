using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Wallets.Commands.CreateDeposit;

public class CreateDepositCommand : ICommand<CreateDepositCommandResponse>
{
    public decimal Amount { get; set; }
}
