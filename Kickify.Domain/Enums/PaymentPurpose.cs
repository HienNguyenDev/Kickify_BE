namespace Kickify.Domain.Enums
{
    public enum PaymentPurpose
    {
        /// <summary>
        /// User is topping up their wallet balance.
        /// </summary>
        Deposit,

        /// <summary>
        /// User is paying the check-in deposit for a match room directly via VNPay
        /// (bypasses wallet balance).
        /// </summary>
        CheckIn
    }
}
