using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Common;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

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
            AccountHolderName = w.Wallet?.AccountHolderName,
            FeeRatePercent = GetFeeRatePercent(w.Wallet?.WalletType),
            FeeAmount = GetFeeAmount(w.Amount, w.Wallet?.WalletType),
            NetPayoutAmount = GetNetPayoutAmount(w.Amount, w.Wallet?.WalletType)
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

    private static decimal? GetFeeRatePercent(WalletType? walletType) =>
        walletType == WalletType.VenueOwner
            ? PlatformConstants.WithdrawalFeeRate * 100
            : null;

    private static decimal? GetFeeAmount(decimal amount, WalletType? walletType)
    {
        if (walletType != WalletType.VenueOwner)
            return null;

        return Math.Min(
            Math.Round(amount * PlatformConstants.WithdrawalFeeRate, 0),
            PlatformConstants.WithdrawalFeeCap);
    }

    private static decimal? GetNetPayoutAmount(decimal amount, WalletType? walletType)
    {
        var feeAmount = GetFeeAmount(amount, walletType);
        return feeAmount.HasValue ? amount - feeAmount.Value : null;
    }
}
