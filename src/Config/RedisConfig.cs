namespace RateLimit.Redis.Sample.Config;

internal sealed class RedisConfig
{
    public string Configuration { get; set; } = "localhost:6379,abortConnect=false";
}