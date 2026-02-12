namespace Kickify.Application.Features.Withdrawals.Queries.GetMyWithdrawals;

public class GetMyWithdrawalsQueryResponse
{
    public List<WithdrawalDto> Withdrawals { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class WithdrawalDto
{
    public Guid WithdrawalId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? AdminNotes { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? AccountHolderName { get; set; }
}
