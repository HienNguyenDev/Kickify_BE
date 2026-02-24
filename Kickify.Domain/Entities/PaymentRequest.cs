using Kickify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Entities
{
    public class PaymentRequest
    {
        public Guid PaymentRequestId { get; set; }
        public string TxnRef { get; set; } = null!;
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? VnpayTransactionNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime ExpiredAt { get; set; }

        // Navigation
        public User? User { get; set; } = null!;
        public Wallet? Wallet { get; set; }
    }
}
