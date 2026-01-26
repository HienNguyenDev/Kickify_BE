using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;

namespace Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;

public class ProcessDepositIpnCommand : ICommand<ProcessDepositIpnCommandResponse>
{
    public VnPayCallbackData CallbackData { get; set; } = null!;
}
