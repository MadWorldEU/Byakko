namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Guid>
{
    public Name Name { get; private set; } = null!;
    
    /// <summary>
    /// Required for EF Core
    /// </summary>
    [UsedImplicitly]
    private Asset() {}

    private Asset(Guid id, Name name)                                                                                                                           
    {           
        Id = id;
        Name = name;
    }

    
    public static Result<Asset> Create(Name name)
    {
        return new Asset(Guid.NewGuid(), name);
    }
}