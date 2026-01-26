using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IPaymentRequestRepository : IGenericRepository<PaymentRequest>
    {
        Task<PaymentRequest?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default);
        Task<List<PaymentRequest>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
