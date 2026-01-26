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
    public class PaymentRequestRepository : GenericRepository<PaymentRequest>, IPaymentRequestRepository
    {
        public PaymentRequestRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PaymentRequest?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentRequests
                .FirstOrDefaultAsync(p => p.TxnRef == txnRef, cancellationToken);
        }

        public async Task<List<PaymentRequest>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentRequests
                .Where(p => p.UserId == userId &&
                           p.Status == PaymentStatus.Pending &&
                           p.ExpiredAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}
