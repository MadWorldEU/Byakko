namespace MadWorldEU.Byakko.Storages;

public record FileRequest(string FileName, string ContentType, long SizeInBytes, Stream Content);