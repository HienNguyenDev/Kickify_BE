using Kickify.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Kickify.Application.Abstractions.Services;

public interface IVnPayService
{
    (string Url, string TxnRef) CreatePaymentUrl(decimal amount, string description);
    VnPayCallbackData ProcessCallback(IQueryCollection query);
}

public class VnPayPaymentRequest
{
    public long PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

public class VnPayPaymentResult
{
    public long PaymentId { get; set; }

    public string Description { get; set; }

    public DateTime Timestamp { get; set; }

    public long VnpayTransactionId { get; set; }

    public string CardType { get; set; }

    public BankingInformation BankingInfor { get; set; }
}
public class BankingInformation
{
    public string BankCode { get; set; }

    public string BankTransactionId { get; set; }
}