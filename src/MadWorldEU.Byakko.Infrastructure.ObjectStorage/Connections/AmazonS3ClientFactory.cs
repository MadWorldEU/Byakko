namespace MadWorldEU.Byakko.Connections;

internal static class AmazonS3ClientFactory
{
    internal static IAmazonS3 Create(IConfiguration configuration)
    {
        var mode = configuration["Storage:Mode"];

        var settings = mode switch
        {
            "LocalStack" => CreateLocalStackSettings(configuration),
            "OvhCloud" => CreateOvhCloudSettings(configuration),
            _ => throw new InvalidOperationException($"Unknown storage mode: {mode}")
        };

        var config = new AmazonS3Config
        {
            ServiceURL = settings.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = settings.Region,
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
        };

        return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
    }

    private static ObjectStorageSettings CreateLocalStackSettings(IConfiguration configuration)
    {
        var endpoint = configuration.GetConnectionString("localstack")
            ?? throw new InvalidOperationException("ConnectionStrings:localstack is not configured.");

        return new ObjectStorageSettings
        {
            Endpoint = endpoint,
            AccessKey = "test",
            SecretKey = "test",
            Region = "us-east-1"
        };
    }
    
    private static ObjectStorageSettings CreateOvhCloudSettings(IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}