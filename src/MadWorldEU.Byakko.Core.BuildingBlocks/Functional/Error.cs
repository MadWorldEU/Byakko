namespace MadWorldEU.Byakko.Functional;

/// <summary>Represents a named error with a human-readable description.</summary>
public sealed class Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Description { get; }

    private Error(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static Error Create(string code, string description) => new(code, description);
}