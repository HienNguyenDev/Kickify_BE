using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Premium.Commands.PurchasePremiumByWallet;

public record PurchasePremiumByWalletCommand : ICommand<PurchasePremiumByWalletResponse>;