using System.Security.Cryptography;
using System.Text;
using RateLimit.Redis.Sample.Config;

namespace RateLimit.Redis.Sample.Services.HashToken;

internal sealed class HashTokenService : IHashTokenService
{
    public string? GetToken(HttpContext http, CookieConfig cookieConfig)
    {
        if (http.Request.Cookies.TryGetValue(cookieConfig.Name, out var existing) &&
            !string.IsNullOrWhiteSpace(existing))
            return existing;
        return null;
    }

    public string ComputeToken(CookieConfig cookieConfig, string ip, string userAgent)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(cookieConfig.Secret));
        return Convert.ToHexString(h
            .ComputeHash(Encoding.UTF8.GetBytes($"{ip}|{userAgent}")))
        .ToLowerInvariant();
    }

    public string GetClientIp(HttpContext ctx)
    {
        var fwd = ctx.Request.Headers["X-Forwarded-For"].ToString();
        var ip = !string.IsNullOrWhiteSpace(fwd)
            ? fwd.Split(',')[0].Trim()
            : (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        if (ip == "::1") ip = "127.0.0.1"; // normalização útil em dev
        return ip;
    }
}