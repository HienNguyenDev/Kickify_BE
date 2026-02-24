using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.DTOs
{
    public class VnPayCallbackData
    {
        public string TxnRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public string TransactionNo { get; set; } = null!;
        public string BankCode { get; set; } = null!;
        public long VnpayTransactionId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsSuccess => ResponseCode == "00";

    }
}
