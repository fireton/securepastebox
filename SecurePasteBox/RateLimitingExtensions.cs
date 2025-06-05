using System.Threading.RateLimiting;

namespace SecurePasteBox;

public static class RateLimitingExtensions
{
    private const string PolicyName = "KeyRetrievalPolicy";

    public static IServiceCollection AddUniversalRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PolicyName, httpContext =>
            {
                var fingerprint = GetClientFingerprint(httpContext);
                return RateLimitPartition.GetTokenBucketLimiter(fingerprint, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 1,
                    TokensPerPeriod = 1,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(5),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }

    public static RouteHandlerBuilder WithKeyRetrievalRateLimit(this RouteHandlerBuilder builder)
    {
        return builder.RequireRateLimiting(PolicyName);
    }

    private static string GetClientFingerprint(HttpContext context)
    {
        var headers = context.Request.Headers;
        var forwardedFor = headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        var realIp = headers["X-Real-IP"].FirstOrDefault();
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();

        var ip = forwardedFor ?? realIp ?? remoteIp ?? "unknown";
        var userAgent = headers.UserAgent.ToString();

        return $"{ip}:{userAgent}".ToLowerInvariant();
    }
}