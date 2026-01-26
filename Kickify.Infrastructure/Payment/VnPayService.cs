using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Microsoft.AspNetCore.Http;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace Kickify.Infrastructure.Payment;

public class VnPayService : IVnPayService
{
    private readonly IVnpayClient _vnpayClient;

    public VnPayService(IVnpayClient vnpayClient)
    {
        _vnpayClient = vnpayClient;
    }

    public (string Url, string TxnRef) CreatePaymentUrl(decimal amount, string description)
    {
        var request = new VnpayPaymentRequest
        {
            Money = (double)amount,
            Description = description,
            BankCode = BankCode.ANY,
            Language = DisplayLanguage.Vietnamese
        };

        var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);

        var txnRef = $"{request.PaymentId}{Random.Shared.Next(100, 999)}";

        return (paymentUrlInfo.Url, txnRef);
    }
    public VnPayCallbackData ProcessCallback(IQueryCollection query)
    {
        var libraryResult = _vnpayClient.GetPaymentResult(query);
        var vnpAmount = query["vnp_Amount"].ToString();
        var vnpResponseCode = query["vnp_ResponseCode"].ToString();
        var vnpTransactionStatus = query["vnp_TransactionStatus"].ToString();

        decimal amount = 0;
        if (long.TryParse(vnpAmount, out var rawAmount))
        {
            amount = rawAmount / 100m;
        }

        var callbackData = new VnPayCallbackData
        {
            //library
            PaymentId = libraryResult.PaymentId,
            Description = libraryResult.Description,
            VnpayTransactionId = libraryResult.VnpayTransactionId,
            BankCode = libraryResult.BankingInfor?.BankCode,

            //query
            Amount = amount,
            ResponseCode = vnpResponseCode,
            TransactionStatus = vnpTransactionStatus
        };

        return callbackData;
    }

    //public bool ValidateCallback(VnPayPaymentResult result)
    //{
    //    return result.IsSuccess && result.IsVerified;
    //}
}