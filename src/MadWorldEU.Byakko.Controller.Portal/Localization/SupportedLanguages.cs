namespace MadWorldEU.Byakko.Localization;

public static class SupportedLanguages
{
    public const string DefaultLanguage = "en";
    public const string DefaultUiLanguage = "EN";
    
    public static Dictionary<string, string> Labels { get; } = new()
    {
        { DefaultLanguage, DefaultUiLanguage },
        { "nl-NL", "NL" },
        { "ja-JP", "JA" }
    };
}