using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : ICommand<DeleteUserCommandResponse>
    {
        public Guid UserId { get; set; }
    }
}
