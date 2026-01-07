using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
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
    }
}
