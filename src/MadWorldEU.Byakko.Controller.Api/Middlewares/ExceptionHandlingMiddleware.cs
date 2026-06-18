using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Middlewares;

/// <summary>
/// Catches unhandled exceptions and returns a structured <see cref="FailureResponse"/> with HTTP 500.
/// </summary>
internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await WriteFailureResponseAsync(context);
        }
    }

    private static async Task WriteFailureResponseAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var failure = new FailureResponse
        {
            Code = "Server.InternalError",
            StatusCode = StatusCodes.Status500InternalServerError,
            Description = "An unexpected error occurred. Please try again later."
        };

        await context.Response.WriteAsJsonAsync(failure);
    }
}