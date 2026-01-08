namespace Kickify.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandResponse
    {
        public Guid UserId { get; set; }
        public string Message { get; set; } = "User deleted successfully";
        public DateTime DeletedAt { get; set; }
    }
}
