namespace Kickify.Api.Requests
{
    /// <summary>
    /// Request to block a time slot on a field
    /// Used by venue owners for offline bookings or maintenance
    /// </summary>
    public record BlockFieldSlotRequest
    {
        /// <summary>
        /// The date to block the slot
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// Start time of the blocked slot (e.g., "17:00")
        /// </summary>
        public TimeSpan StartTime { get; init; }

        /// <summary>
        /// End time of the blocked slot (e.g., "18:30")
        /// </summary>
        public TimeSpan EndTime { get; init; }

        /// <summary>
        /// Reason for blocking (e.g., "Maintenance", "Walk-in Customer", "Offline Guest")
        /// </summary>
        public string Reason { get; init; } = string.Empty;

        /// <summary>
        /// Optional amount to track offline revenue (default 0)
        /// </summary>
        public decimal Amount { get; init; } = 0;
    }
}
