using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class BanUserRequest
{
    /// <summary>
    /// Ban duration: OneDay, ThreeDays, SevenDays, ThirtyDays, Permanent
    /// </summary>
    public BanDuration Duration { get; set; }

    /// <summary>
    /// Optional reason for ban
    /// </summary>
    public string? Reason { get; set; }
}
