using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IQuery<GetAllUsersQueryResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
    }
}
