namespace MadWorldEU.Byakko.Extensions;

internal static class ClaimsPrincipalExtensions
{
    internal static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? string.Empty;

    internal static Guid GetUserIdAsGuid(this ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        return string.IsNullOrEmpty(userId) 
            ? Guid.Empty 
            : Guid.Parse(userId);
    }
}