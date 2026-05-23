namespace MadWorldEU.Byakko.Connections;

/// <summary>Configuration options for S3-compatible object storage.</summary>
internal sealed class StorageOptions
{
    internal const string SectionName = "Storage";

    /// <summary>Name of the S3 bucket used to store asset content.</summary>
    internal string BucketName { get; init; } = string.Empty;

    /// <summary>When true, <see cref="BucketInitializer"/> creates the bucket on startup if it does not exist.</summary>
    internal bool AutoCreateBucket { get; init; }
}