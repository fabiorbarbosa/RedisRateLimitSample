using RateLimit.Redis.Sample.Config;

namespace RateLimit.Redis.Sample.Services.HashToken;

internal interface IHashTokenService
{
    string? GetToken(HttpContext http, CookieConfig cookieConfig);
    string ComputeToken(CookieConfig cookieConfig, string ip, string userAgent);
    string GetClientIp(HttpContext ctx);
}