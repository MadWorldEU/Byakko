using System.Security.Cryptography;

namespace MadWorldEU.Byakko.Encryptions;

/// <summary>AES-256-CBC encryption service. The IV is randomly generated per call and prepended to the ciphertext.</summary>
public sealed class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        _key = Convert.FromBase64String(options.Value.Key);
    }

    public string Decrypt(string encryptedContent)
    {
        using var input = new MemoryStream(Convert.FromBase64String(encryptedContent));
        using var output = (MemoryStream)Decrypt(input);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    public Stream Decrypt(Stream encryptedContent)
    {
        using var aes = Aes.Create();
        aes.Key = _key;

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

    public string Encrypt(string content)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var output = (MemoryStream)Encrypt(input);
        return Convert.ToBase64String(output.ToArray());
    }

    public Stream Encrypt(Stream content)
    {
        using var aes = Aes.Create();
        aes.Key = _key;

        var output = new MemoryStream();
        output.Write(aes.IV);

        using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            content.CopyTo(cryptoStream);
        }

        output.Position = 0;
        return output;
    }
}