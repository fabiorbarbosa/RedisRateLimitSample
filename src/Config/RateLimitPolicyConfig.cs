namespace RateLimit.Redis.Sample.Config;

internal sealed class RateLimitPolicyConfig
{
    public List<RateLimitConfig> Policies { get; set; } = [];
}
