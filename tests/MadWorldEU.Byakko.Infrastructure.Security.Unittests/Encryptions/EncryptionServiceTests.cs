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
    
    [Test]
    public void Encrypt_WhenGivenValidString_ShouldBeDecryptableToOriginalString()
    {
        var encryptionService = new EncryptionService(_settings);
        
        var encryptedString = encryptionService.Encrypt(CorrectDecryptedString);  
        var decryptedString = encryptionService.Decrypt(encryptedString);
        
        decryptedString.ShouldBe(CorrectDecryptedString);
        decryptedString.ShouldNotBe(encryptedString);
    }
    
    [Test]
    public void Decrypt_WhenGivenEncryptedString_ShouldBeDecryptableToOriginalString()
    {
        const string encryptedString = "QEaj5F11lbhEztxqkt0ssWdlKBsiVzRdF7PZezWdM+nFrRM4WZJwZ9DJZWIr21lIBtlF6qkEkmMRhVZ28iYyHP3hG+fdM4bd9fTRUTe+kPzJ+jdvIJCz/cQ2tYXSxFfcMiK/aNrR0jfqN62KeHuULNTzAzvfUSO40KNRruGExVPVWw3ADoWYGj7nfzXN3sP7nSR5xKA6GlWtT/0RtFU8mg==";
        
        var encryptionService = new EncryptionService(_settings);
        
        var decryptedString = encryptionService.Decrypt(encryptedString);
        decryptedString.ShouldBe(CorrectDecryptedString);
    }
}