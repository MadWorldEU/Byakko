namespace MadWorldEU.Byakko.Development;

public sealed class GetEnvironmentVariablesResponse
{
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();
}