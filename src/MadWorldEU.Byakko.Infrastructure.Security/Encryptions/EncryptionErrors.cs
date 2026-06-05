namespace MadWorldEU.Byakko.Encryptions;

/// <summary>Errors for the encryption infrastructure service.</summary>
internal static class EncryptionErrors
{
    internal static readonly Error DecryptionFailed = Error.Create("Encryption.DecryptionFailed", "The content could not be decrypted.");
}