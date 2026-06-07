namespace MadWorldEU.Byakko.Common.Pages;

/// <summary>Represents the number of items per page for paginated queries.</summary>
public sealed class PageSize : ValueObject
{
    public const int MaxSize = 100;
    public int Value { get; }

    private PageSize(int value)
    {
        Value = value;
    }

    public static Result<PageSize> Create(int size)
    {
        if (size <= 0 || size > MaxSize) return PageSizeErrors.Invalid;
        return new PageSize(size);
    }

    public static implicit operator int(PageSize pageSize) => pageSize.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}