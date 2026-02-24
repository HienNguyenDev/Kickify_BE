using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Withdrawals.Queries.GetAllWithdrawals;

public class GetAllWithdrawalsQuery : IQuery<GetAllWithdrawalsQueryResponse>
{
    public WithdrawalStatus? Status { get; set; }
    public WalletType? WalletType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
