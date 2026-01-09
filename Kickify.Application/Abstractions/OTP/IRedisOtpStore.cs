using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.OTP
{
    public interface IRedisOtpStore
    {
        Task StoreAsync(Guid userId, string otp, TimeSpan timeSpan, CancellationToken cancellationToken);
        Task<string?> GetAsync(Guid userId, CancellationToken cancellationToken);
        Task RemoveAsync(Guid userId, CancellationToken cancellationToken);
    }
}
