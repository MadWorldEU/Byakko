namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Id>
{
    public const string DefaultPath = "assets";
    
    public Name Name { get; private set; } = null!;
    public ContentType ContentType { get; private set; } = null!;
    public UserId CreatedBy { get; private init; } = null!;
    public Size Size { get; private set; } = null!;
    public Instant CreatedAt { get; private init; }
    public Instant UpdatedAt { get; private set; }
    
    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Asset() {}

    private Asset(Id id, Name name, ContentType contentType, UserId createdBy, Instant createdAt)
    {
        Id = id;
        Name = name;
        ContentType = contentType;
        CreatedBy = createdBy;
        Size = Size.Create(0).Value;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Result<Asset> Create(IClock clock, IGuidGenerator guidGenerator, Name name, ContentType contentType, UserId createdBy)
    {
        var now = clock.GetCurrentInstant();
        var id = Id.Create(guidGenerator.New()).Value;
        return new Asset(id, name, contentType, createdBy, now);
    }

    public void UpdateSize(Size size, Instant updatedAt)
    {
        Size = size;
        UpdatedAt = updatedAt;
    }

    public AssetPath GetPath() => AssetPath.Create(DefaultPath, Id.Value.ToString()).Value;
}