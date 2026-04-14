using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Withdrawals.Commands.CancelWithdrawal;
using Kickify.Application.Features.Withdrawals.Commands.CreateWithdrawal;
using Kickify.Application.Features.Withdrawals.Commands.ProcessWithdrawal;
using Kickify.Application.Features.Withdrawals.Queries.GetAllWithdrawals;
using Kickify.Application.Features.Withdrawals.Queries.GetMyWithdrawals;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/withdrawals")]
[ApiController]
[Authorize]
public class WithdrawalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WithdrawalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> CreateWithdrawal(
        [FromBody] CreateWithdrawalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(w => $"/api/withdrawals/{w.WithdrawalId}");
    }

    [HttpPost("{withdrawalId:guid}/cancel")]
    public async Task<IResult> CancelWithdrawal(
        Guid withdrawalId,
        CancellationToken cancellationToken)
    {
        var command = new CancelWithdrawalCommand { WithdrawalId = withdrawalId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("my")]
    public async Task<IResult> GetMyWithdrawals(
        [FromQuery] WithdrawalStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyWithdrawalsQuery
        {
            Status = status,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetAllWithdrawals(
        [FromQuery] WithdrawalStatus? status = null,
        [FromQuery] WalletType? walletType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWithdrawalsQuery
        {
            Status = status,
            WalletType = walletType,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{withdrawalId:guid}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> ProcessWithdrawal(
        Guid withdrawalId,
        [FromBody] ProcessWithdrawalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessWithdrawalCommand
        {
            WithdrawalId = withdrawalId,
            IsApproved = request.IsApproved,
            AdminNotes = request.AdminNotes
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
 