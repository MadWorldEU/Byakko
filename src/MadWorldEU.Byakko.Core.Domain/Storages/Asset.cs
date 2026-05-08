namespace MadWorldEU.Byakko.Storages;

public sealed class Asset : Entity<Guid>
{
    public Name Name { get; private set; } = null!;
    
    private Asset() {} // for EF Core   

    private Asset(Guid id, Name name)                                                                                                                           
    {           
        Id = id;
        Name = name;
    }

    
    public Asset Create(Name name)
    {
        return new Asset(Guid.NewGuid(), name);
    }
}