namespace MadWorldEU.Byakko.Common;

public sealed class Name : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private Name() { } // for EF Core   

    private Name(string value)
    {
        Value = value;
    }

    public Name Create(string name)
    {
        return new Name(name);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}