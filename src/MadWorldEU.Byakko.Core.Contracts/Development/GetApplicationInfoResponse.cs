namespace MadWorldEU.Byakko.Development;

public sealed class GetApplicationInfoResponse
{
    public required string ApplicationName { get; init; } = string.Empty;
    public required string ApplicationVersion { get; init; } = string.Empty;
    public required string ApplicationPath { get; init; } = string.Empty;
    public required string MachineName { get; init; } = string.Empty;
    public required string OsDescription { get; init; } = string.Empty;
    public required string RuntimeVersion { get; init; } = string.Empty;
    public required int ProcessId { get; init; }
    public required DateTime StartTime { get; init; }
    public required string Environment { get; init; } = string.Empty;
}