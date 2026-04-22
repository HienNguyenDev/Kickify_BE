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

        /// <summary>
        /// Purpose of this payment: Deposit (top-up wallet) or CheckIn (direct room check-in via VNPay).
        /// </summary>
        public PaymentPurpose Purpose { get; set; } = PaymentPurpose.Deposit;

        /// <summary>
        /// Populated when Purpose == CheckIn. Points to the room being paid for.
        /// </summary>
        public Guid? RoomId { get; set; }

        // Navigation
        public User? User { get; set; } = null!;
        public Wallet? Wallet { get; set; }
        public MatchRoom? Room { get; set; }
    }
}
