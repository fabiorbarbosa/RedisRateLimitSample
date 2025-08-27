using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using RateLimit.Redis.Sample.Services.HashToken;
using RateLimit.Redis.Sample.Config;

namespace RateLimit.Redis.Sample.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false, Inherited = true)]
public sealed class EnsureRateLimitTokenAttribute(string policyName)
    : ActionFilterAttribute
{
    private readonly string _policyName = policyName;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var root = http.RequestServices.GetRequiredService<IOptions<RateLimitPolicyConfig>>().Value;
        var policy = root.Policies.FirstOrDefault(p => p.PolicyName == _policyName);
        if (policy is null)
        {
            await next();
            return;
        }

        var tokenSvc = http.RequestServices.GetRequiredService<IHashTokenService>();
        var mux = http.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var db = mux.GetDatabase();

        // Cookie/token
        var ip = tokenSvc.GetClientIp(http);
        var token = http.Request.Cookies[policy.Cookie.Name];
        if (string.IsNullOrWhiteSpace(token))
        {
            var ua = http.Request.Headers.UserAgent.ToString();
            token = tokenSvc.ComputeToken(policy.Cookie, ip, ua);

            var cookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = policy.Cookie.Secure,
                Path = policy.Cookie.Path ?? "/",
                SameSite = (policy.Cookie.SameSite?
                    .Equals("None", StringComparison.OrdinalIgnoreCase) ?? false)
                        ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };
            http.Response.Cookies.Append(policy.Cookie.Name, token, cookie);
        }

        var partition = $"{ip}:{token}";
        var hashKey = $"rl:fw:{_policyName}:{partition}"; // ✅ SEM windowStart

        // Provisiona hash se não existir
        if (!await db.KeyExistsAsync(hashKey))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var ws = policy.WindowSeconds;
            var windowStart = now - (now % ws);

            await db.HashSetAsync(hashKey,
            [
                new("windowStart", windowStart),
                new("count",       0L)
            ]);

            await db.KeyExpireAsync(hashKey, TimeSpan.FromDays(1)); // TTL do token
        }

        await next();
    }
}
