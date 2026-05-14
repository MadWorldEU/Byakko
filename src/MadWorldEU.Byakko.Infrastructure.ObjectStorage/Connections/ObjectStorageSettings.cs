namespace MadWorldEU.Byakko.Connections;

internal sealed class ObjectStorageSettings
{
    internal required string Endpoint { get; init; }
    internal required string AccessKey { get; init; }
    internal required string SecretKey { get; init; }
    internal required string Region { get; init; }
}