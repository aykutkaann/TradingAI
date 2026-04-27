using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Infrastructure.Cache;

/// <summary>
/// No-op cache. Used when Redis is not configured (e.g. Railway free tier
/// where Redis fails to deploy). Every Get returns null; Set/Remove no-op.
/// The app stays correct, just hits the underlying providers more often.
/// </summary>
public class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken ct)
        => Task.CompletedTask;
}
