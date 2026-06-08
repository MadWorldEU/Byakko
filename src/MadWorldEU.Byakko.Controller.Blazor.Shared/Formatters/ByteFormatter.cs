using System.Globalization;

namespace MadWorldEU.Byakko.Formatters;

/// <summary>Formats byte counts into human-readable size strings.</summary>
public static class ByteFormatter
{
    /// <summary>Formats a nullable byte count into a human-readable string (e.g. "1.2 MB"). Returns "—" when null.</summary>
    public static string Format(long? bytes)
    {
        if (bytes is null) return "—";
        var totalBytes = bytes.Value;
        return totalBytes switch
        {
            < 1024 => $"{totalBytes} B",
            < 1024 * 1024 => (totalBytes / 1024.0).ToString("F1", CultureInfo.InvariantCulture) + " KB",
            < 1024 * 1024 * 1024 => (totalBytes / (1024.0 * 1024)).ToString("F1", CultureInfo.InvariantCulture) + " MB",
            _ => (totalBytes / (1024.0 * 1024 * 1024)).ToString("F2", CultureInfo.InvariantCulture) + " GB"
        };
    }
}