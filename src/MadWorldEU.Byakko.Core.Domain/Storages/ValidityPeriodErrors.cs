namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the ValidityPeriod value object.</summary>
public static class ValidityPeriodErrors
{
    public static readonly Error MustBePositive = Error.Create("ValidityPeriod.MustBePositive", "Validity period must be at least one day.");
    public static readonly Error ExceedsMaximum = Error.Create("ValidityPeriod.ExceedsMaximum", "Validity period exceeds the maximum allowed days.");
}