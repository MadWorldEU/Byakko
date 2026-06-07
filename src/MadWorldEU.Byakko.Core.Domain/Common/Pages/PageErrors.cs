namespace MadWorldEU.Byakko.Common.Pages;

/// <summary>Errors for the Page value object.</summary>
public static class PageErrors
{
    public static readonly Error Invalid = Error.Create("Page.Invalid", "Page number must be greater than zero.");
}