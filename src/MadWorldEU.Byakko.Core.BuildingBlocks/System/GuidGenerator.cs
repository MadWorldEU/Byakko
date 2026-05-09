namespace MadWorldEU.Byakko.System;

public sealed class GuidGenerator : IGuidGenerator
{
    public Guid New()
    {
        return Guid.NewGuid();
    }
}