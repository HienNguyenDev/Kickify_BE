using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Wallets.Commands.UpdateBankInfo;

public class UpdateBankInfoCommand : ICommand<UpdateBankInfoCommandResponse>
{
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}
