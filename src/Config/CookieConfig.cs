namespace RateLimit.Redis.Sample.Config;

internal sealed class CookieConfig
{
    public string Name { get; set; } = "rlk";
    public string Secret { get; set; } = "change-me";
    public bool Secure { get; set; } = true;
    public string? SameSite { get; set; } = "Lax";
    public string? Path { get; set; } = "/";
}
