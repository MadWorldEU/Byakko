using System.Threading.RateLimiting;

namespace MadWorldEU.Byakko.Configurations;

/// <summary>Registers the global rate limiting policy for the Status service.</summary>
internal static class RateLimiterExtensions
{
    /// <summary>Adds a global fixed-window limiter applied to all endpoints, partitioned by IP address. Limits are read from <c>RateLimiting</c> in appsettings.</summary>
    internal static IServiceCollection AddStatusRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimiterSettings.Key).Get<RateLimiterSettings>()
            ?? throw new InvalidOperationException($"Missing configuration section '{RateLimiterSettings.Key}'.");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.General.PermitLimit,
                        Window = TimeSpan.FromSeconds((double)settings.General.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}