namespace MadWorldEU.Byakko.Development;

/// <summary>Response containing all environment variables visible to the running process.</summary>
public sealed class GetEnvironmentVariablesResponse
{
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();
}