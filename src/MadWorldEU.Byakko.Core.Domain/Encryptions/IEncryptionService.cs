namespace MadWorldEU.Byakko.Encryptions;

public interface IEncryptionService
{
    string Decrypt(string encryptedContent);
    Stream Decrypt(Stream encryptedContent);
    string Encrypt(string content);
    Stream Encrypt(Stream content);
}