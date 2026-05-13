namespace MadWorldEU.Byakko.Storages;

public sealed class ContentType : ValueObject
{
    public const int MaxLength = 256;
    public string Value { get; }

    private ContentType(string value)
    {
        Value = value;
    }

    public static Result<ContentType> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return ContentTypeErrors.Empty;
        if (contentType.Length > MaxLength) return ContentTypeErrors.TooLong;
        return new ContentType(contentType);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}