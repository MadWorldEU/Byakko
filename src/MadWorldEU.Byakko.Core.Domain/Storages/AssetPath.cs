namespace MadWorldEU.Byakko.Storages;

public sealed class AssetPath : ValueObject
{
    public const int MaxLength = 1024;

    public string Path { get; }
    public string Key { get; }
    public string FullPath => Path + "/" + Key;

    private AssetPath(string path, string key)
    {
        Path = path;
        Key = key;   
    }

    public static Result<AssetPath> Create(string path, string key)
    {
        if (string.IsNullOrWhiteSpace(path)) return AssetPathErrors.PathEmpty;
        if (path.Length > MaxLength) return AssetPathErrors.PathTooLong;

        if (string.IsNullOrWhiteSpace(key)) return AssetPathErrors.KeyEmpty;
        if (key.Length > MaxLength) return AssetPathErrors.KeyTooLong;

        return new AssetPath(path, key);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Path;
        yield return Key;
    }
}