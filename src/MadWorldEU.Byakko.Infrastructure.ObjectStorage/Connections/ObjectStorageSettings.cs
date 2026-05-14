namespace MadWorldEU.Byakko.Connections;

internal sealed class ObjectStorageSettings
{
    public required string Endpoint { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string Region { get; init; }
}