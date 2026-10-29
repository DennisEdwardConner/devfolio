using StackExchange.Redis;

namespace DevFolio.Infrastructure.Services;

public class RedisService
{
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<long> IncrementProjectViewAsync(int projectId)
    {
        return await _db.StringIncrementAsync($"project:{projectId}:views");
    }

    public async Task<long> GetProjectViewsAsync(int projectId)
    {
        var val = await _db.StringGetAsync($"project:{projectId}:views");
        return val.HasValue ? (long)val : 0;
    }

    public async Task<bool> IsRateLimitedAsync(string ipAddress)
    {
        var key = $"ratelimit:{ipAddress}";
        var count = await _db.StringIncrementAsync(key);

        if (count == 1)
        {
            await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(15));
        }

        return count > 3;
    }
}