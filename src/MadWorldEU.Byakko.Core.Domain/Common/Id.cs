namespace MadWorldEU.Byakko.Common;

public sealed class Id : ValueObject
{
    public Guid Value { get; }
    public bool IsEmpty => Value == Guid.Empty;
    
    private Id(Guid value)
    {
        Value = value;
    }

    public static Result<Id> Create(Guid id)
    {
        if (id == Guid.Empty)
            return IdErrors.Empty;

        return new Id(id);
    }

    public static Result<Id> Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return IdErrors.Empty;

        if (!Guid.TryParse(id, out var guid))
            return IdErrors.Invalid;

        return Create(guid);
    }
    
    public static Id Empty => new(Guid.Empty);
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}