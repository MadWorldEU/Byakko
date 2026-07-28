using Microsoft.Extensions.Caching.Memory;

namespace MadWorldEU.Byakko.AuthenticationServers;

/// <summary>In-memory caching decorator for <see cref="IAuthenticationRepository"/>.</summary>
public sealed class CachedAuthenticationRepository(
    IAuthenticationRepository inner,
    IMemoryCache cache) : IAuthenticationRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(1);

    /// <summary>Deletes the user from the authentication server and evicts the cache entry.</summary>
    public async Task<Result> DeleteUser(UserId userId)
    {
        var result = await inner.DeleteUser(userId);

        if (result.IsSuccess)
        {
            cache.Remove(CacheKey(userId));
        }

        return result;
    }

    /// <summary>Returns the user from cache when available; otherwise fetches from the authentication server and caches the result.</summary>
    public async Task<Result<AuthenticationUser>> FindUser(UserId userId)
    {
        if (cache.TryGetValue(CacheKey(userId), out AuthenticationUser? cached) && cached is not null)
        {
            return cached;
        }

        var result = await inner.FindUser(userId);

        if (result.IsSuccess)
        {
            cache.Set(CacheKey(userId), result.Value, CacheDuration);
        }

        return result;
    }

    private static string CacheKey(UserId userId) => $"auth-user:{userId.Value}";
}