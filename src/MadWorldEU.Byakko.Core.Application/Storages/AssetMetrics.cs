using System.Diagnostics.Metrics;

namespace MadWorldEU.Byakko.Storages;

/// <summary>Tracks asset-related metrics for uploads and downloads.</summary>
public sealed class AssetMetrics : IAssetMetrics
{
    public const string MeterName = "Byakko.Assets";

    private readonly Counter<long> _uploadsTotal;
    private readonly Counter<long> _downloadsTotal;

    public AssetMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _uploadsTotal = meter.CreateCounter<long>(
            "assets.uploads.total",
            description: "Total number of successful asset uploads.");
        _downloadsTotal = meter.CreateCounter<long>(
            "assets.downloads.total",
            description: "Total number of successful asset downloads.");
    }

    public void RecordUpload() => _uploadsTotal.Add(1);
    public void RecordDownload() => _downloadsTotal.Add(1);
}