namespace Kickify.Application.Features.Withdrawals.Queries.GetAllWithdrawals;

public class GetAllWithdrawalsQueryResponse
{
    public List<AdminWithdrawalDto> Withdrawals { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdminWithdrawalDto
{
    public Guid WithdrawalId { get; set; }
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? ProcessedByAdminName { get; set; }
    public string? AdminNotes { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
    public decimal? FeeRatePercent { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? NetPayoutAmount { get; set; }
}
