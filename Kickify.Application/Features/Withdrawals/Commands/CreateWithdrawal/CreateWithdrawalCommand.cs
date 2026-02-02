using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Withdrawals.Commands.CreateWithdrawal;

public class CreateWithdrawalCommand : ICommand<CreateWithdrawalCommandResponse>
{
    public decimal Amount { get; set; }
}
