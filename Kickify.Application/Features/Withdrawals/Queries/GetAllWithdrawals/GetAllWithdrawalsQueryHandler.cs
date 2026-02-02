using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Withdrawals.Queries.GetAllWithdrawals;

public class GetAllWithdrawalsQueryHandler : IQueryHandler<GetAllWithdrawalsQuery, GetAllWithdrawalsQueryResponse>
{
    private readonly IWalletWithdrawalRepository _withdrawalRepository;

    public GetAllWithdrawalsQueryHandler(IWalletWithdrawalRepository withdrawalRepository)
    {
        _withdrawalRepository = withdrawalRepository;
    }

    public async Task<Result<GetAllWithdrawalsQueryResponse>> Handle(
        GetAllWithdrawalsQuery request,
        CancellationToken cancellationToken)
    {
        var (withdrawals, total) = await _withdrawalRepository.GetAllAsync(
            request.Status,
            request.WalletType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var withdrawalDtos = withdrawals.Select(w => new AdminWithdrawalDto
        {
            WithdrawalId = w.WithdrawalId,
            WalletId = w.WalletId,
            UserId = w.Wallet?.UserId ?? Guid.Empty,
            UserFullName = w.Wallet?.User?.FullName,
            UserEmail = w.Wallet?.User?.Email,
            WalletType = w.Wallet?.WalletType.ToString() ?? string.Empty,
            Amount = w.Amount,
            Status = w.Status.ToString(),
            RequestDate = w.RequestDate,
            ProcessedDate = w.ProcessedDate,
            ProcessedByAdminName = w.ProcessedByAdmin?.FullName,
            AdminNotes = w.AdminNotes,
            BankAccountNumber = w.Wallet?.BankAccountNumber,
            BankName = w.Wallet?.BankName,
            AccountHolderName = w.Wallet?.AccountHolderName
        }).ToList();

        return Result.Success(new GetAllWithdrawalsQueryResponse
        {
            Withdrawals = withdrawalDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }
}
