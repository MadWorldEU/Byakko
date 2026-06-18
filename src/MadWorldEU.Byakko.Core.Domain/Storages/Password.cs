namespace MadWorldEU.Byakko.Storages;

public sealed class Password : ValueObject
{
    public string Value { get; }

    private Password(string? value)
    {
        Value = value ?? string.Empty;
    }
    
    public static Result<Password> Create(string? password)
    {
        return new Password(password);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}