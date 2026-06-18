using MadWorldEU.Byakko.Storages;

namespace MadWorldEU.Byakko.Encryptions;

public interface IEncryptionService
{
    Result<string> Decrypt(string encryptedContent, Password password);
    Result<Stream> Decrypt(Stream encryptedContent, Password password);
    string Encrypt(string content, Password password);
    Stream Encrypt(Stream content, Password password);
}