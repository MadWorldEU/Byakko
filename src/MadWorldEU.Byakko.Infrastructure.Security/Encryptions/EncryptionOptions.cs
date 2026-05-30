namespace MadWorldEU.Byakko.Encryptions;

/// <summary>Configuration options for AES-256 encryption.</summary>
public sealed class EncryptionOptions
{
    public const string SectionName = "Encryption";

    /// <summary>Base64-encoded 32-byte AES-256 encryption key.</summary>
    public string Key { get; init; } = string.Empty;
}