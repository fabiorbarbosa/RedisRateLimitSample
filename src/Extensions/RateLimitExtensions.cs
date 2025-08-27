using Microsoft.Extensions.Options;
using StackExchange.Redis;
using RateLimit.Redis.Sample.Config;
using RateLimit.Redis.Sample.Services.HashToken;
using RateLimit.Redis.Sample.Services.RedisFixedWindow;

namespace RateLimit.Redis.Sample.Extensions;

internal static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimitInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RateLimitPolicyConfig>(config.GetSection("RateLimiting"));
        services.Configure<RedisConfig>(config.GetSection("Redis"));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<IOptions<RedisConfig>>().Value;
            return ConnectionMultiplexer.Connect(cfg.Configuration);
        });

        services.AddSingleton<IHashTokenService, HashTokenService>();
        services.AddSingleton<IRedisFixedWindowService, RedisFixedWindowService>();

        return services;
    }
}
