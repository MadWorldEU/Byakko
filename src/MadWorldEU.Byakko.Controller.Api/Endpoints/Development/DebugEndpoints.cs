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
    }
}