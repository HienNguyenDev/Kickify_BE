using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Withdrawals.Commands.CancelWithdrawal;

public class CancelWithdrawalCommand : ICommand<CancelWithdrawalCommandResponse>
{
    public Guid WithdrawalId { get; set; }
}
