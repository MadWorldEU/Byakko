namespace MadWorldEU.Byakko.Storages;

public sealed class CreateAssetRequest
{
    public string Name { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}