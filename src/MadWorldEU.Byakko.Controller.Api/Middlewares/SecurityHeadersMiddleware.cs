namespace MadWorldEU.Byakko.Middlewares;

/// <summary>Appends security response headers to every HTTP response.</summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>Adds security headers and invokes the next middleware.</summary>
    internal async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        await next(context);
    }
}