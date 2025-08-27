namespace RateLimit.Redis.Sample.Config;

internal sealed class RateLimitConfig
{
    public string PolicyName { get; set; } = "FixedApi";
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
    public CookieConfig Cookie { get; set; } = new();
    public string TokenEndpoint { get; set; } = "/auth/issue-token";
}
