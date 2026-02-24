using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        var txnRef = request.PaymentId.ToString();

        return (paymentUrlInfo.Url, txnRef);
    }

    public VnPayCallbackData? ProcessCallback(IQueryCollection query)
    {
        try
        {
            var txnRef = query["vnp_TxnRef"].ToString();
            var vnpAmount = query["vnp_Amount"].ToString();
            var vnpResponseCode = query["vnp_ResponseCode"].ToString();
            var vnpTransactionNo = query["vnp_TransactionNo"].ToString();
            var vnpBankCode = query["vnp_BankCode"].ToString();
            var vnpTransactionStatus = query["vnp_TransactionStatus"].ToString();

            if (string.IsNullOrEmpty(txnRef) || string.IsNullOrEmpty(vnpResponseCode))
            {
                return null;
            }

            decimal amount = 0;
            if (long.TryParse(vnpAmount, out var rawAmount))
            {
                amount = rawAmount / 100m;
            }

            long.TryParse(vnpTransactionNo, out var transactionId);

            var result = new VnPayCallbackData
            {
                TxnRef = txnRef,
                Amount = amount,
                ResponseCode = vnpResponseCode,
                TransactionStatus = string.IsNullOrEmpty(vnpTransactionStatus) ? vnpResponseCode : vnpTransactionStatus,
                TransactionNo = vnpTransactionNo ?? "",
                BankCode = vnpBankCode ?? "",
                VnpayTransactionId = transactionId,
                IsVerified = true  
            };

            return result;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}