namespace Kickify.Api.Requests
{
    public class UpdateFormationRequest
    {
        public required string Team { get; set; }  // "A" or "B"
        public required string FormationName { get; set; }  // e.g., "4-3-3"
        public required List<FormationSlotAssignmentRequest> Assignments { get; set; }
    }

    public class FormationSlotAssignmentRequest
    {
        public Guid PlayerId { get; set; }
        public required string SlotId { get; set; }  // e.g., "GK-0", "DF-1", "MF-2", "FW-0"
    }
}
