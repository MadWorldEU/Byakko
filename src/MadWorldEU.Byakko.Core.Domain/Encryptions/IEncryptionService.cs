namespace MadWorldEU.Byakko.Encryptions;

public interface IEncryptionService
{
    Result<string> Decrypt(string encryptedContent);
    Result<Stream> Decrypt(Stream encryptedContent);
    string Encrypt(string content);
    Stream Encrypt(Stream content);
}