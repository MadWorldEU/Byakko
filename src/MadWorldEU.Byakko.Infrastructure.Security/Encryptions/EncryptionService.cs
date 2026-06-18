using System.Security.Cryptography;
using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Encryptions;

/// <summary>AES-256-CBC encryption service. The IV is randomly generated per call and prepended to the ciphertext.</summary>
public sealed class EncryptionService(IOptions<EncryptionOptions> options, ILogger<EncryptionService> logger) : IEncryptionService
{
    private readonly byte[] _key = Convert.FromBase64String(options.Value.Key);

    public Result<string> Decrypt(string encryptedContent, Password password)
    {
        using var input = new MemoryStream(Convert.FromBase64String(encryptedContent));
        var contentResult = Decrypt(input, password);
        
        if (contentResult.IsFailure) 
            return Result.Failure<string>(contentResult.Error);
        
        using var output = (MemoryStream)contentResult.Value;
        return Encoding.UTF8.GetString(output.ToArray());
    }

    public Result<Stream> Decrypt(Stream encryptedContent, Password password)
    {
        try
        {
            var salt = new byte[16];
            encryptedContent.ReadExactly(salt);
            using var aes = DeriveKey(password, salt);

            var iv = new byte[aes.BlockSize / 8];
            encryptedContent.ReadExactly(iv);
            aes.IV = iv;

            var output = new MemoryStream();

            using (var cryptoStream = new CryptoStream(encryptedContent, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(output);
            }

            output.Position = 0;
            return output;
        }
        catch (Exception ex) when (ex is CryptographicException or EndOfStreamException)
        {
            logger.LogError(ex, "Failed to decrypt content.");
            return Result.Failure<Stream>(EncryptionErrors.DecryptionFailed);
        }
    }

    public string Encrypt(string content, Password password)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var output = (MemoryStream)Encrypt(input, password);
        return Convert.ToBase64String(output.ToArray());
    }

    public Stream Encrypt(Stream content, Password password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var aes = DeriveKey(password, salt);
        var output = new MemoryStream();
        output.Write(salt);
        output.Write(aes.IV);

        using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            content.CopyTo(cryptoStream);
        }

        output.Position = 0;
        return output;
    }

    private Aes DeriveKey(Password password, byte[] salt)
    {
        var aes = Aes.Create();
        var combined = _key.Concat(Encoding.UTF8.GetBytes(password.Value)).ToArray();
        aes.Key = Rfc2898DeriveBytes.Pbkdf2(combined, salt, iterations: 600_000, HashAlgorithmName.SHA256, outputLength: 32);
        return aes;
    }
}