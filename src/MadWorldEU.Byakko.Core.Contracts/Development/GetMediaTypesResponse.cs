namespace MadWorldEU.Byakko.Development;

/// <summary>Response containing all known MIME types.</summary>
public sealed class GetMediaTypesResponse
{
    public IEnumerable<string> MediaTypes { get; init; } = [];
}