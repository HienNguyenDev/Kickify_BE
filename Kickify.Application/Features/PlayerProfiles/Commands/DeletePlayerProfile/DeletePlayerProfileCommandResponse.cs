namespace Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile
{
    public class DeletePlayerProfileCommandResponse
    {
        public Guid ProfileId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
