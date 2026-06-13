namespace MadWorldEU.Byakko.Extensions;

internal static class ClaimsPrincipalExtensions
{
    internal static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? string.Empty;
}