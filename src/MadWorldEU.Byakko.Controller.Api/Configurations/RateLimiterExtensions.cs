using System.Security.Claims;
using System.Threading.RateLimiting;

namespace MadWorldEU.Byakko.Configurations;

/// <summary>Registers rate limiting policies for the API.</summary>
internal static class RateLimiterExtensions
{
    /// <summary>Adds a global fixed-window limiter applied to all endpoints, plus a stricter content policy for upload and download. Limits are read from <c>RateLimiting</c> in appsettings.</summary>
    internal static IServiceCollection AddApiRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimiterSettings.Key).Get<RateLimiterSettings>()
            ?? throw new InvalidOperationException($"Missing configuration section '{RateLimiterSettings.Key}'.");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.General.PermitLimit,
                        Window = TimeSpan.FromSeconds((double)settings.General.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimiterPolicies.Content, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.Content.PermitLimit,
                        Window = TimeSpan.FromSeconds((double)settings.Content.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimiterPolicies.PublicPost, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PublicPost.PermitLimit,
                        Window = TimeSpan.FromSeconds((double)settings.PublicPost.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;
        return userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}