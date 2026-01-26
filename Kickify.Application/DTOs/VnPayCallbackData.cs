using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.DTOs
{
    public class VnPayCallbackData
    {
        public long PaymentId { get; set; }
        public string Description { get; set; } = null!;
        public long VnpayTransactionId { get; set; }
        public string? BankCode { get; set; }
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public string TxnRef => PaymentId.ToString();
        public bool IsSuccess => ResponseCode == "00" && TransactionStatus == "00";
    }
}
