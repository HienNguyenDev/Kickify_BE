using Kickify.Api.Extensions;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Wallets.Commands.CreateDeposit;
using Kickify.Application.Features.Wallets.Commands.ProcessDepositIpn;
using Kickify.Application.Features.Wallets.Queries.GetAllWalletTransactions;
using Kickify.Application.Features.Wallets.Queries.GetWalletBalance;
using Kickify.Application.Features.Wallets.Queries.GetWalletTransactions;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/wallets")]
[ApiController]
public class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IVnPayService _vnPayService;

    public WalletsController(IMediator mediator, IVnPayService vnPayService)
    {
        _mediator = mediator;
        _vnPayService = vnPayService;
    }

    [HttpGet("balance")]
    [Authorize]
    public async Task<IResult> GetWalletBalance(CancellationToken cancellationToken)
    {
        var query = new GetWalletBalanceQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("transactions")]
    [Authorize]
    public async Task<IResult> GetWalletTransactions(
        [FromQuery] TransactionType? transactionType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWalletTransactionsQuery
        {
            TransactionType = transactionType,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("transactions/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetAllWalletTransactions(
        [FromQuery] WalletType? walletType = null,
        [FromQuery] TransactionType? transactionType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWalletTransactionsQuery
        {
            WalletType = walletType,
            TransactionType = transactionType,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("deposit")]
    [Authorize]
    public async Task<IActionResult> CreateDeposit(
        [FromBody] CreateDepositCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("ipn")]
    public async Task<IActionResult> IpnCallback(CancellationToken cancellationToken)
    {
        try
        {
            var callbackData = _vnPayService.ProcessCallback(Request.Query);

            if (callbackData == null)
            {
                return Ok(new { RspCode = "97", Message = "Invalid data" });
            }

            var command = new ProcessDepositIpnCommand { CallbackData = callbackData };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(new
            {
                RspCode = result.Value?.RspCode ?? "99",
                Message = result.Value?.Message ?? "Unknown error"
            });
        }
        catch
        {
            return Ok(new { RspCode = "99", Message = "Server error" });
        }
    }

    [HttpGet("callback")]
    public IActionResult PaymentCallback()
    {
        var callbackData = _vnPayService.ProcessCallback(Request.Query);

        var redirectUrl = callbackData.IsSuccess
            ? $"https://kickify.site/payment/success?txnRef={callbackData.TxnRef}&amount={callbackData.Amount}"
            : $"https://kickify.site/payment/failure?code={callbackData.ResponseCode}";

        return Redirect(redirectUrl);
    }
}
