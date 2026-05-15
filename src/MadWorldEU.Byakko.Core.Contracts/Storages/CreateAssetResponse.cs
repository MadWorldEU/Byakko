namespace MadWorldEU.Byakko.Storages;

/// <summary>Response returned after successfully creating a new asset metadata record.</summary>
public sealed class CreateAssetResponse
{
    public required Guid Id { get; init; }
}