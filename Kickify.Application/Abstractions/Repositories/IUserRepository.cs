using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailExistsAsync(string email);
        Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default);
        Task<(IEnumerable<User> Users, int Total)> GetPagedUsersAsync(
            UserRole? role = null,
            bool? isActive = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
        Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
