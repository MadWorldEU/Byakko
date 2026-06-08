namespace MadWorldEU.Byakko.Common;

public sealed class UserId : ValueObject
{
    public Guid Value { get; }
    public bool IsEmpty => Value == Guid.Empty;

    private UserId(Guid value)
    {
        Value = value;
    }

    public static Result<UserId> Create(Guid id)
    {
        if (id == Guid.Empty)
        {
            return UserIdErrors.Empty;
        }

        return new UserId(id);
    }

    public static Result<UserId> Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return UserIdErrors.Empty;
        }

        if (!Guid.TryParse(id, out var guid))
        {
            return UserIdErrors.Invalid;
        }

        return Create(guid);
    }
    
    public static UserId Empty => new (Guid.Empty);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}