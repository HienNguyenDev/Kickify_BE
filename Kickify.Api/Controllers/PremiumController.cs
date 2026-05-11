using Kickify.Api.Extensions;
using Kickify.Application.Features.Premium.Commands.PurchasePremium;
using Kickify.Application.Features.Premium.Commands.PurchasePremiumByWallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/premium")]
[Authorize]
public class PremiumController : ControllerBase
{
    private readonly ISender _sender;

    public PremiumController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Initiate a Premium subscription purchase (49,000 VND / 30 days) via VNPay.
    /// Returns a VNPay payment URL — redirect the user to complete payment.
    /// Premium is activated automatically via the IPN callback after successful payment.
    /// </summary>
    [HttpPost("purchase")]
    public async Task<IResult> PurchasePremium(CancellationToken cancellationToken)
    {
        var command = new PurchasePremiumCommand();
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Purchase a Premium subscription using the current user's in-app wallet balance.
    /// Premium is activated immediately if the wallet has sufficient balance.
    /// </summary>
    [HttpPost("purchase/wallet")]
    public async Task<IResult> PurchasePremiumByWallet(CancellationToken cancellationToken)
    {
        var command = new PurchasePremiumByWalletCommand();
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
