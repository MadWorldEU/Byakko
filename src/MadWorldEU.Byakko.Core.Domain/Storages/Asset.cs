namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Guid>
{
    public Name Name { get; private set; } = null!;
    public Instant CreatedAt { get; private init; }
    
    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Asset() {}

    private Asset(Guid id, Name name, Instant createdAt)                                                                                                                           
    {           
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    
    public static Result<Asset> Create(IClock clock, IGuidGenerator guidGenerator, Name name)
    {
        var now = clock.GetCurrentInstant();
        return new Asset(guidGenerator.New(), name, now);
    }
}