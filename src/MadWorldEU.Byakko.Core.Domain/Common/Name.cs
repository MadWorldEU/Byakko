namespace MadWorldEU.Byakko.Common;

public sealed class Name : ValueObject
{
    public const int MaxLength = 256;
    public string Value { get; private init; } = string.Empty;
    
    private Name() { } // for EF Core   

    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return NameErrors.Empty;
        if (name.Length > MaxLength) return NameErrors.TooLong;
        return new Name(name);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}