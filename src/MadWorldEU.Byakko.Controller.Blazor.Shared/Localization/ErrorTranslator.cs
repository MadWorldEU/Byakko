using MadWorldEU.Byakko.Common;
using Microsoft.Extensions.Localization;

namespace MadWorldEU.Byakko.Localization;

/// <summary>
/// Translates a <see cref="FailureResponse"/> error code to a localized message, falling back to the response description when no resource key matches.
/// </summary>
internal sealed class ErrorTranslator(IStringLocalizer<ErrorResources> localizer) : IErrorTranslator
{
    /// <inheritdoc/>
    public string Translate(FailureResponse response)
    {
        return Translate(response.Code, response.Description);
    }

    /// <inheritdoc/>
    public string Translate(string code, string defaultDescription)
    {
        var localizedString = localizer[code];
        if (localizedString.ResourceNotFound)
        {
            return defaultDescription;
        }

        return localizedString;
    }
}