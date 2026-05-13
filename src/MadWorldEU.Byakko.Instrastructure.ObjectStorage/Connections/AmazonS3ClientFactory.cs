namespace MadWorldEU.Byakko.Connections;

internal static class AmazonS3ClientFactory
{
    internal static IAmazonS3 Create(IConfiguration configuration)
    {
        var mode = configuration["Storage:Mode"];

        var settings = mode switch
        {
            "Minio" => CreateMinioSettings(configuration),
            "OvhCloud" => CreateOvhCloudSettings(configuration),
            _ => throw new InvalidOperationException($"Unknown storage mode: {mode}")
        };

        var config = new AmazonS3Config
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true, // important for MinIO
            AuthenticationRegion = settings.Region
        };

        return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
    }

    private static ObjectStorageSettings CreateMinioSettings(IConfiguration configuration)
    {
        var parts = configuration.GetConnectionString("minio")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => p[1]);
        
        return new ObjectStorageSettings
        {
            Endpoint = parts["Endpoint"],
            AccessKey = parts["AccessKey"],
            SecretKey = parts["SecretKey"],
            Region = "eu-west-1"
        };       
    }
    
    private static ObjectStorageSettings CreateOvhCloudSettings(IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}