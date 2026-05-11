namespace Kickify.Application.Common;

/// <summary>
/// Centralised platform monetisation constants.
/// Change rates here and they propagate to all handlers automatically.
/// </summary>
public static class PlatformConstants
{
    /// <summary>5 % commission deducted from every confirmed booking before paying the venue owner.</summary>
    public const decimal BookingCommissionRate = 0.05m;

    /// <summary>1 % withdrawal fee charged when a venue owner withdraws their balance.</summary>
    public const decimal WithdrawalFeeRate = 0.01m;

    /// <summary>Maximum withdrawal fee in VND (cap).</summary>
    public const decimal WithdrawalFeeCap = 50_000m;

    /// <summary>Price of the monthly Premium subscription (VND).</summary>
    public const decimal PremiumPriceVnd = 49_000m;

    /// <summary>Premium subscription duration.</summary>
    public static readonly TimeSpan PremiumDuration = TimeSpan.FromDays(30);
}
