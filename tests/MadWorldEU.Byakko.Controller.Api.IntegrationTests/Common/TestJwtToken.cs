using System.Security.Cryptography;
using System.Text;

namespace MadWorldEU.Byakko.Common;

/// <summary>Generates minimal JWT tokens for integration tests. Only valid when Authentication:ValidateUser is false.</summary>
internal static class TestJwtToken
{
    private static readonly byte[] Key = "integration-test-signing-key-at-least-32-chars!!"u8.ToArray();

    internal static string Create(string userId, string[]? roles = null)
    {
        roles ??= ["Administrator", "User"];
        var header = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var rolesJson = $"[{string.Join(",", roles.Select(r => $"\"{r}\""))}]";
        var payload = Base64UrlEncode($"{{\"sub\":\"{userId}\",\"exp\":{exp},\"roles\":{rolesJson}}}");
        var signingInput = Encoding.UTF8.GetBytes($"{header}.{payload}");
        using var hmac = new HMACSHA256(Key);
        var signature = Base64UrlEncode(hmac.ComputeHash(signingInput));
        return $"{header}.{payload}.{signature}";
    }

    private static string Base64UrlEncode(string value) =>
        Base64UrlEncode(Encoding.UTF8.GetBytes(value));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}