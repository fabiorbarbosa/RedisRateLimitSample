using StackExchange.Redis;

namespace RateLimit.Redis.Sample.Services.RedisFixedWindow;


internal sealed class RedisFixedWindowService : IRedisFixedWindowService
{
    private readonly IDatabase _db;
    private static readonly TimeSpan TokenTtl = TimeSpan.FromDays(1); // TTL do token/hash

    public RedisFixedWindowService(IConnectionMultiplexer mux)
        => _db = mux.GetDatabase();

    public async Task<(bool acquired, int retryAfterSeconds)> TryAcquireAsync(
        string policyName, string partitionKey, int permitLimit, int windowSeconds)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - (now % windowSeconds);

        // ✅ chave SEM windowStart no nome
        var hashKey = $"rl:fw:{policyName}:{partitionKey}";

        if (!await _db.KeyExistsAsync(hashKey))
            return (false, -1); // não provisionada => 403

        var vals = await _db.HashGetAsync(hashKey, new RedisValue[] { "windowStart", "count" });
        var curWin = vals[0]; var curCnt = vals[1];
        if (curWin.IsNullOrEmpty || curCnt.IsNullOrEmpty)
            return (false, -1); // campos ausentes => trate como não provisionada

        long curWindowStart = (long)curWin;
        long curCount = (long)curCnt;

        if (curWindowStart != windowStart)
        {
            // virou a janela: reset lógico
            await _db.HashSetAsync(hashKey, new HashEntry[]
            {
                new("windowStart", windowStart),
                new("count",       1L)
            });
            await _db.KeyExpireAsync(hashKey, TokenTtl);
            return (true, 0);
        }

        // mesma janela: incrementa
        long newCount = await _db.HashIncrementAsync(hashKey, "count", 1L);
        if (newCount <= permitLimit)
        {
            await _db.KeyExpireAsync(hashKey, TokenTtl);
            return (true, 0);
        }

        // limite estourado → quanto falta para virar
        var elapsed = (int)(now - windowStart);
        var secondsLeft = Math.Max(1, windowSeconds - elapsed);
        return (false, secondsLeft);
    }
}