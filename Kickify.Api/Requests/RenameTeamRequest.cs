namespace Kickify.Api.Requests
{
    public class RenameTeamRequest
    {
        public required string Team { get; set; }  // "A" or "B"
        public string? Name { get; set; }  // Team name, e.g., "FC Real Madrid"
    }
}
