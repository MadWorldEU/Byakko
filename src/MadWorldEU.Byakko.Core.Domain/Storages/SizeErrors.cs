namespace MadWorldEU.Byakko.Storages;

/// <summary>Errors for the Size value object.</summary>
public static class SizeErrors
{
    public static readonly Error Negative = Error.Create("Size.Negative", "Size cannot be negative.");
}