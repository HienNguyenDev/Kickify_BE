using Kickify.Application.Abstractions.OTP;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Redis
{
    public class RedisOtpStore : IRedisOtpStore
    {
        private readonly IDatabase _db;
        private const string Prefix = "otp:";

        public RedisOtpStore(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

        public Task StoreAsync(Guid userId, string otp, TimeSpan timeSpan, CancellationToken cancellationToken) => _db.StringSetAsync($"{Prefix}{userId}", otp, expiry: timeSpan);

        public async Task<string?> GetAsync(Guid userId, CancellationToken cancellationToken)
        {
            var v = await _db.StringGetAsync($"{Prefix}{userId}");
            return v.IsNullOrEmpty ? null : v.ToString();
        }

        public Task RemoveAsync(Guid userId, CancellationToken cancellationToken)
            => _db.KeyDeleteAsync($"{Prefix}{userId}");
    }
}
