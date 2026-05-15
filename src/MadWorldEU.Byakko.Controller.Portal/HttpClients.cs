namespace MadWorldEU.Byakko;

/// <summary>
/// Named HTTP client identifiers for use with IHttpClientFactory.
/// </summary>
internal static class HttpClients
{
    internal const string ApiAnonymous = nameof(ApiAnonymous);
    internal const string ApiAuthorized = nameof(ApiAuthorized);
}