namespace Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;

public class ProcessDepositIpnCommandResponse
{
    public bool Success { get; set; }
    public string RspCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
