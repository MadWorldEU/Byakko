namespace MadWorldEU.Byakko;

/// <summary>
/// Named HTTP client identifiers for use with IHttpClientFactory.
/// </summary>
public static class HttpClients
{
    public const string ApiAnonymous = nameof(ApiAnonymous);
    public const string ApiAuthorized = nameof(ApiAuthorized);
}