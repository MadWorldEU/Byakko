namespace MadWorldEU.Byakko.Connections;

/// <summary>Configuration options for S3-compatible object storage.</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Name of the S3 bucket used to store asset content.</summary>
    public string BucketName { get; init; } = string.Empty;

    /// <summary>When true, <see cref="BucketInitializer"/> creates the bucket on startup if it does not exist.</summary>
    public bool AutoCreateBucket { get; init; }

    /// <summary>OVHCloud S3-compatible storage credentials. Required when <see cref="Mode"/> is <c>OvhCloud</c>.</summary>
    public OvhCloudOptions OvhCloud { get; init; } = new();
}

/// <summary>OVHCloud S3-compatible connection settings.</summary>
public sealed class OvhCloudOptions
{
    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
}