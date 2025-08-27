namespace RateLimit.Redis.Sample.Services.RedisFixedWindow;

internal interface IRedisFixedWindowService
{
    Task<(bool acquired, int retryAfterSeconds)> TryAcquireAsync(string policyName,
        string partitionKey, int permitLimit, int windowSeconds);
}