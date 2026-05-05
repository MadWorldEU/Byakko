using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MadWorldEU.Byakko.Development;

namespace MadWorldEU.Byakko.Endpoints.Development;

public static class DebugEndpoints
{
    public static void AddDebugEndpoints(this WebApplication app)
    {
        var debugEndpoints = app.MapGroup("/debug")
            .WithTags("Debug");
        
        debugEndpoints.MapGet("/environment/variables", () =>
            {
                var variables = Environment.GetEnvironmentVariables();
                return new GetEnvironmentVariablesResponse
                {
                    EnvironmentVariables = variables.Keys.Cast<string>()
                        .OrderBy(k => k)
                        .ToDictionary(k => k, k => variables[k]?.ToString() ?? string.Empty)
                };
            })
            .WithName("GetEnvironmentVariables");
        
        debugEndpoints.MapGet("/info", (IHostEnvironment hostEnvironment) => new GetApplicationInfoResponse
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty,
                ApplicationVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty,
                ApplicationPath = Directory.GetCurrentDirectory(),
                MachineName = Environment.MachineName,
                OsDescription = RuntimeInformation.OSDescription,
                RuntimeVersion = RuntimeInformation.FrameworkDescription,
                ProcessId = Environment.ProcessId,
                StartTime = Process.GetCurrentProcess().StartTime,
                Environment = hostEnvironment.EnvironmentName
            })
            .WithName("GetApplicationInfo");
    }
}