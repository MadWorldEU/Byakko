namespace MadWorldEU.Byakko.Systems;

public sealed class GuidGenerator : IGuidGenerator
{
    public Guid New()
    {
        return Guid.NewGuid();
    }
}