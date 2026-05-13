namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Id>
{
    public const string DefaultPath = "assets";
    
    public Name Name { get; private set; } = null!;
    public ContentType ContentType { get; private set; } = null!;
    public Instant CreatedAt { get; private init; }
    
    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Asset() {}

    private Asset(Id id, Name name, ContentType contentType, Instant createdAt)
    {
        Id = id;
        Name = name;
        ContentType = contentType;
        CreatedAt = createdAt;
    }

    public static Result<Asset> Create(IClock clock, IGuidGenerator guidGenerator, Name name, ContentType contentType)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new Asset(id, name, contentType, now);
    }

    public AssetPath GetPath() => AssetPath.Create(DefaultPath, Id.Value.ToString()).Value;
}