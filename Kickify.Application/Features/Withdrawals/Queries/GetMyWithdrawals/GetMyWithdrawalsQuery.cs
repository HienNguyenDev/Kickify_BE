using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Withdrawals.Queries.GetMyWithdrawals;

public class GetMyWithdrawalsQuery : IQuery<GetMyWithdrawalsQueryResponse>
{
    public WithdrawalStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
