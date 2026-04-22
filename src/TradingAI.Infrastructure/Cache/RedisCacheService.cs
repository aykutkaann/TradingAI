using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

using System.Text.Json;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly StackExchange.Redis.IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
           
        }
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
        {


            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(value);

            await _db.StringSetAsync(key, json, expiry);

        }

        public async Task RemoveAsync(string key, CancellationToken ct)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}
