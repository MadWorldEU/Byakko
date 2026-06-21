namespace MadWorldEU.Byakko.Localization;

/// <summary>
/// Resolves the active culture from a saved preference or browser language.
/// </summary>
internal static class CultureResolver
{
    /// <summary>
    /// Returns the saved culture if valid, otherwise falls back to the browser language.
    /// </summary>
    internal static string Resolve(string savedCulture, string browserLanguage)
    {
        if (!string.IsNullOrEmpty(savedCulture))
        {
            return SupportedLanguages.Labels.Keys.Contains(savedCulture)
                ? savedCulture
                : SupportedLanguages.DefaultLanguage;
        }

        return FromBrowserLanguage(browserLanguage);
    }

    private static string FromBrowserLanguage(string browserLanguage)
    {
        if (string.IsNullOrEmpty(browserLanguage))
        {
            return SupportedLanguages.DefaultLanguage;
        }

        var neutralBrowser = browserLanguage.Split('-')[0];

        foreach (var supported in SupportedLanguages.Labels.Keys)
        {
            if (browserLanguage.StartsWith(supported, StringComparison.OrdinalIgnoreCase))
            {
                return supported;
            }

            var neutralSupported = supported.Split('-')[0];
            if (neutralBrowser.Equals(neutralSupported, StringComparison.OrdinalIgnoreCase))
            {
                return supported;
            }
        }

        return SupportedLanguages.DefaultLanguage;
    }
}