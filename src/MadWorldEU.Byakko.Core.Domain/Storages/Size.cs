namespace MadWorldEU.Byakko.Storages;

/// <summary>File size in bytes.</summary>
public sealed class Size : ValueObject
{
    public long Value { get; }

    private Size(long value)
    {
        Value = value;
    }

    public static Result<Size> Create(long sizeInBytes, long maxSizeInBytes)
    {
        if (sizeInBytes > maxSizeInBytes)
        {
            return AssetErrors.FileTooLarge;
        }
        
        return Create(sizeInBytes);
    }

    public static Result<Size> Create(long sizeInBytes)
    {
        if (sizeInBytes < 0)
        {
            return SizeErrors.Negative;
        }

        return new Size(sizeInBytes);
    }
    
    public static implicit operator long(Size size) => size.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}