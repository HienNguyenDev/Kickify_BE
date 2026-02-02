using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;

public class ProcessWithdrawalCommand : ICommand<ProcessWithdrawalCommandResponse>
{
    public Guid WithdrawalId { get; set; }
    public bool IsApproved { get; set; }
    public string? AdminNotes { get; set; }
}
