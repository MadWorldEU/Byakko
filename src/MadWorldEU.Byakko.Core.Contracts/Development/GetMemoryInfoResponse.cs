namespace MadWorldEU.Byakko.Development;

public sealed class GetMemoryInfoResponse
{
    public required long TotalMemoryBytes { get; init; }
    public required long WorkingSetBytes { get; init; }
    public required int Gen0Collections { get; init; }
    public required int Gen1Collections { get; init; }
    public required int Gen2Collections { get; init; }
}