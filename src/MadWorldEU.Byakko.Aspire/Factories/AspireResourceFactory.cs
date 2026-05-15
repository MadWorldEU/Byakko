namespace MadWorldEU.Byakko.Factories;

internal static class AspireResourceFactory
{
    internal static IResourceBuilder<PostgresDatabaseResource> BuildDatabase(this IDistributedApplicationBuilder distributedApplicationBuilder)
    {
        var dbUsername = distributedApplicationBuilder.AddParameter("db-username", secret: true);
        var dbPassword = distributedApplicationBuilder.AddParameter("db-password", secret: true);

        var postgres = distributedApplicationBuilder.AddPostgres("postgres", dbUsername, dbPassword)
            .WithDataVolume(isReadOnly: false)
            .WithPgAdmin();

        return postgres.AddDatabase("byakko-db");
    }

    internal static IResourceBuilder<MinioContainerResource> BuildMinio(this IDistributedApplicationBuilder builder)
    {
        var minioUsername = builder.AddParameter("minio-username", secret: true);
        var minioPassword = builder.AddParameter("minio-password", secret: true);

        return builder.AddMinioContainer("minio", minioUsername, minioPassword)
            .WithDataVolume();
    }

    internal static IResourceBuilder<KeycloakResource> BuildKeyCloak(this IDistributedApplicationBuilder builder)
    {
        var keyCloakUsername = builder.AddParameter("keycloak-username", secret: true);
        var keyCloakPassword = builder.AddParameter("keycloak-password", secret: true);

        return builder.AddKeycloak("keycloak", 4321, keyCloakUsername, keyCloakPassword)
            .WithRealmImport("./Configurations/realm-export.json")
            .WithDataVolume();
    }
}