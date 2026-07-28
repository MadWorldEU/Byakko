namespace MadWorldEU.Byakko.Common;

/// <summary>Value object representing a Keycloak username.</summary>
public sealed class Username : ValueObject
{
    /// <summary>The raw username string.</summary>
    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>Creates a <see cref="Username"/> or returns <see cref="UsernameErrors.Empty"/> if the value is null or whitespace.</summary>
    public static Result<Username> Create(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return UsernameErrors.Empty;
        }
        
        return new Username(username);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}