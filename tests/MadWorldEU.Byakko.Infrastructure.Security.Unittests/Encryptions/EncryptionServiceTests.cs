using MadWorldEU.Byakko.Storages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace MadWorldEU.Byakko.Encryptions;

/// <summary>Unit tests for <see cref="EncryptionService"/>.</summary>
public sealed class EncryptionServiceTests
{
    private const string CorrectDecryptedString = "The team used a secure file-sharing platform to exchange project documents across different offices without relying on email attachments.";

    /// <summary>
    /// Base64 encoding of the 32-byte AES-256 key: FZUaz@Cw+%M!Jl5VTn6}7l-=J)TcpVwh
    /// </summary>
    private readonly IOptions<EncryptionOptions> _settings = Options.Create(new EncryptionOptions { Key = "RlpVYXpAQ3crJU0hSmw1VlRuNn03bC09SilUY3BWd2g=" });
    private readonly Password _emptyPassword = Password.Create(string.Empty).Value;
    private readonly Password _normalPassword = Password.Create("MySecretPassword123!").Value;
    private readonly Password _wrongPassword = Password.Create("WrongPassword!").Value;
    
    [Test]
    public void Encrypt_WhenGivenValidString_ShouldBeDecryptableToOriginalString()
    {
        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);

        var encryptedString = encryptionService.Encrypt(CorrectDecryptedString, _emptyPassword);
        var decryptedResult = encryptionService.Decrypt(encryptedString, _emptyPassword);

        decryptedResult.Value.ShouldBe(CorrectDecryptedString);
        decryptedResult.Value.ShouldNotBe(encryptedString);
    }
    
    [Test]
    public void Encrypt_WhenGivenValidStringAndValidPassword_ShouldBeDecryptableToOriginalString()
    {
        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);

        var encryptedString = encryptionService.Encrypt(CorrectDecryptedString, _normalPassword);
        var decryptedResult = encryptionService.Decrypt(encryptedString, _normalPassword);

        decryptedResult.Value.ShouldBe(CorrectDecryptedString);
        decryptedResult.Value.ShouldNotBe(encryptedString);
    }
    
    [Test]
    public void Encrypt_WhenGivenValidStringAndWrongPassword_ShouldBeDecryptableToOriginalString()
    {
        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);

        var encryptedString = encryptionService.Encrypt(CorrectDecryptedString, _normalPassword);
        var decryptedResult = encryptionService.Decrypt(encryptedString, _wrongPassword);

        decryptedResult.IsFailure.ShouldBeTrue();
        decryptedResult.Error.Code.ShouldBe("Encryption.DecryptionFailed");
    }

    [Test]
    public void Decrypt_WhenGivenEncryptedString_ShouldBeDecryptableToOriginalString()
    {
        const string encryptedString = "5cEdaA45IaVJlXtekWaC4YMmTLnc6m878FjSTCYxXDkh0zIgq/NivfvtPZQ/c6wVGFZjvn9QxYHFECehPr6tcCbWkIUQOvYigjybTINcJ7ig/ewjuPoBr65P/zLDHVr6TbmMsDr7Xc5f4gPzvllVmSExVZLqgGsE5HLvkLkwK85lUFlIeMDvsvTaJ77UJ8dDGSyXphtXxRacOqGM3mdtDAk3BwuBw1s64AKQ9k6LCC0=";

        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);

        var decryptedResult = encryptionService.Decrypt(encryptedString, _emptyPassword);
        decryptedResult.Value.ShouldBe(CorrectDecryptedString);
    }

    [Test]
    public void Decrypt_WhenStreamIsTooShort_ShouldReturnFailure()
    {
        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);
        using var tooShort = new MemoryStream([1, 2, 3]);

        var result = encryptionService.Decrypt(tooShort, _emptyPassword);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Encryption.DecryptionFailed");
    }

    [Test]
    public void Decrypt_WhenContentIsCorrupted_ShouldReturnFailure()
    {
        var encryptionService = new EncryptionService(_settings, NullLogger<EncryptionService>.Instance);
        using var corrupted = new MemoryStream(new byte[64]);

        var result = encryptionService.Decrypt(corrupted, _emptyPassword);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Encryption.DecryptionFailed");
    }
}