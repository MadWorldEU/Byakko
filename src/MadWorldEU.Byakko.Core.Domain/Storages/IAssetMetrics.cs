namespace MadWorldEU.Byakko.Storages;

public interface IAssetMetrics
{
    void RecordUpload();
    void RecordDownload();
}