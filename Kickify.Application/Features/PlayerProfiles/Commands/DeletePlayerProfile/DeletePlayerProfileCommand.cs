using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile
{
    public class DeletePlayerProfileCommand : ICommand<DeletePlayerProfileCommandResponse>
    {
        public Guid ProfileId { get; set; }
    }
}
