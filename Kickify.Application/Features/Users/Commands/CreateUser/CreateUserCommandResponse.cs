using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
