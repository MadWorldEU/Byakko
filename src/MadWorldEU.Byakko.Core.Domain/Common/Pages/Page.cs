using MadWorldEU.Byakko.Common.Pages;

namespace MadWorldEU.Byakko.Common;

/// <summary>Represents a 1-based page number for paginated queries.</summary>
public sealed class Page : ValueObject
{
    public int Value { get; }

    private Page(int value)
    {
        Value = value;
    }

    public static Result<Page> Create(int page)
    {
        if (page <= 0) return PageErrors.Invalid;
        return new Page(page);
    }

    public static implicit operator int(Page page) => page.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}