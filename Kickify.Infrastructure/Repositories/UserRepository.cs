using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _dbSet
                .AnyAsync(u => u.Email == email);
        }

        public async Task<(IEnumerable<User> Users, int Total)> GetPagedUsersAsync(
            UserRole? role = null,
            bool? isActive = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            // Note: Global query filter already excludes soft-deleted users (DeletedAt == null)
            //var query = _dbSet.AsNoTracking().AsQueryable();
            IQueryable<User> query = _dbSet.AsNoTracking();



            // Filter by role
            if (role.HasValue)
            {

                query = query.Where(u => u.Role == role.Value);
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.IgnoreQueryFilters().Where(u => u.IsActive == isActive.Value);
            }

            // Search by email, full name, or phone
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(lowerSearchTerm) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(lowerSearchTerm)) ||
                    (u.Phone != null && u.Phone.Contains(searchTerm))
                );
            }

            var total = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, total);
        }

        public async Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Note: Global query filter already excludes soft-deleted users
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.PlayerProfile)
                .Include(u => u.NotificationPreference)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }
    }
}

