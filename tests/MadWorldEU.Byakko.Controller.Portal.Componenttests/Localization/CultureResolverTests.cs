using MadWorldEU.Byakko.Localization;
using Shouldly;

namespace MadWorldEU.Byakko.Localization;

/// <summary>Unit tests for CultureResolver.</summary>
public sealed class CultureResolverTests
{
    [Test]
    [Arguments("en")]
    [Arguments("nl-NL")]
    [Arguments("ja-JP")]
    public void Resolve_WhenSavedCultureIsValid_ShouldReturnSavedCulture(string savedCulture)
    {
        var result = CultureResolver.Resolve(savedCulture, string.Empty);

        result.ShouldBe(savedCulture);
    }

    [Test]
    [Arguments("fr-FR")]
    [Arguments("de")]
    [Arguments("zh-CN")]
    public void Resolve_WhenSavedCultureIsUnsupported_ShouldReturnDefault(string savedCulture)
    {
        var result = CultureResolver.Resolve(savedCulture, string.Empty);

        result.ShouldBe(SupportedLanguages.DefaultLanguage);
    }

    [Test]
    [Arguments("nl-NL")]
    [Arguments("ja-JP")]
    [Arguments("en")]
    public void Resolve_WhenSavedCultureIsEmpty_AndBrowserLanguageIsExactMatch_ShouldReturnMatchedCulture(string browserLanguage)
    {
        var result = CultureResolver.Resolve(string.Empty, browserLanguage);

        result.ShouldBe(browserLanguage);
    }

    [Test]
    [Arguments("nl", "nl-NL")]
    [Arguments("ja", "ja-JP")]
    public void Resolve_WhenSavedCultureIsEmpty_AndBrowserLanguageIsNeutral_ShouldReturnMatchedCulture(string browserLanguage, string expected)
    {
        var result = CultureResolver.Resolve(string.Empty, browserLanguage);

        result.ShouldBe(expected);
    }

    [Test]
    [Arguments("nl-BE", "nl-NL")]
    [Arguments("ja-JP", "ja-JP")]
    [Arguments("en-US", "en")]
    [Arguments("en-GB", "en")]
    public void Resolve_WhenSavedCultureIsEmpty_AndBrowserLanguageIsRegionalVariant_ShouldReturnMatchedCulture(string browserLanguage, string expected)
    {
        var result = CultureResolver.Resolve(string.Empty, browserLanguage);

        result.ShouldBe(expected);
    }

    [Test]
    [Arguments("fr-FR")]
    [Arguments("de")]
    [Arguments("zh-CN")]
    public void Resolve_WhenSavedCultureIsEmpty_AndBrowserLanguageIsUnsupported_ShouldReturnDefault(string browserLanguage)
    {
        var result = CultureResolver.Resolve(string.Empty, browserLanguage);

        result.ShouldBe(SupportedLanguages.DefaultLanguage);
    }

    [Test]
    public void Resolve_WhenBothEmpty_ShouldReturnDefault()
    {
        var result = CultureResolver.Resolve(string.Empty, string.Empty);

        result.ShouldBe(SupportedLanguages.DefaultLanguage);
    }

    [Test]
    [Arguments("NL-NL", "nl-NL")]
    [Arguments("JA-JP", "ja-JP")]
    public void Resolve_WhenSavedCultureIsEmpty_AndBrowserLanguageCasingDiffers_ShouldStillMatch(string browserLanguage, string expected)
    {
        var result = CultureResolver.Resolve(string.Empty, browserLanguage);

        result.ShouldBe(expected);
    }
}