namespace MadWorldEU.Byakko.Common.Pages;

/// <summary>Errors for the PageSize value object.</summary>
public static class PageSizeErrors
{
    public static readonly Error Invalid = Error.Create("PageSize.Invalid", $"Page size must be between 1 and {PageSize.MaxSize}.");
}