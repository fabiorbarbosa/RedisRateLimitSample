using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using RateLimit.Redis.Sample.Config;
using RateLimit.Redis.Sample.Services.HashToken;
using RateLimit.Redis.Sample.Services.RedisFixedWindow;

namespace RateLimit.Redis.Sample.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false, Inherited = true)]
public sealed class FixedWindowRateLimitAttribute(string policyName)
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
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        var store = http.RequestServices.GetRequiredService<IRedisFixedWindowService>();
        var tokenSvc = http.RequestServices.GetRequiredService<IHashTokenService>();
        var ip = tokenSvc.GetClientIp(http);
        var token = tokenSvc.GetToken(http, policy.Cookie);

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        var partition = $"{ip}:{token}";
        var (ok, retryAfter) = await store.TryAcquireAsync(_policyName,
            partition, policy.PermitLimit, policy.WindowSeconds);

        if (!ok)
        {
            if (retryAfter == -1)
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            else
            {
                http.Response.Headers["Retry-After"] = retryAfter.ToString();
                context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
            }
            return;
        }

        await next();
    }
}
