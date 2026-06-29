namespace MadWorldEU.Byakko.Storages;

/// <summary>Number of days an asset remains valid after creation.</summary>
public sealed class ValidityPeriod : ValueObject
{
    public int Days { get; }

    private ValidityPeriod(int days)
    {
        Days = days;
    }

    public static Result<ValidityPeriod> Create(int days, int maxDays)
    {
        if (days > maxDays)
        {
            return ValidityPeriodErrors.ExceedsMaximum;
        }
        
        return Create(days);
    }
    
    public static Result<ValidityPeriod> Create(int days)
    {
        if (days <= 0)
        {
            return ValidityPeriodErrors.MustBePositive;
        }

        return new ValidityPeriod(days);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Days;
    }
}