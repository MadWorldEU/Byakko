namespace MadWorldEU.Byakko.Formatters;

/// <summary>Formats byte counts into human-readable size strings.</summary>
public static class ByteFormatter
{
    /// <summary>Formats a nullable byte count into a human-readable string (e.g. "1.2 MB"). Returns "—" when null.</summary>
    public static string Format(long? bytes)
    {
        if (bytes is null) return "—";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}