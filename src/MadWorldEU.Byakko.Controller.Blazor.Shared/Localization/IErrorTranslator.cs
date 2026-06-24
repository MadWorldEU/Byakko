using MadWorldEU.Byakko.Common;

namespace MadWorldEU.Byakko.Localization;

/// <summary>
/// Translates a <see cref="FailureResponse"/> error code to a localized message.
/// </summary>
public interface IErrorTranslator
{
    /// <summary>
    /// Returns the localized string for <paramref name="response"/>'s error code, or the raw description if the key is not found.
    /// </summary>
    string Translate(FailureResponse response);

    /// <summary>
    /// Returns the localized string for <paramref name="code"/>, or <paramref name="defaultDescription"/> if the key is not found.
    /// </summary>
    string Translate(string code, string defaultDescription);
}