using Kickify.Domain.Enums;

namespace Kickify.Api.Requests
{
    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? PreferredPositions { get; set; }
        public int? ShirtNumber { get; set; }
        public string? PreferredFoot { get; set; }
    }
}
