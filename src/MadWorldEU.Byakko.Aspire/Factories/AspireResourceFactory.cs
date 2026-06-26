using Aspire.Hosting.LocalStack.Container;

namespace MadWorldEU.Byakko.Factories;

internal static class AspireResourceFactory
{
    private const int KeyCloakPort = 4321;
    private const int KeyCloakSecurePort = 4322;
    private const int LocalStackPort = 4421;
    
    internal static IResourceBuilder<PostgresDatabaseResource> BuildDatabase(this IDistributedApplicationBuilder distributedApplicationBuilder)
    {
        var dbUsername = distributedApplicationBuilder.AddParameter("db-username", secret: true);
        var dbPassword = distributedApplicationBuilder.AddParameter("db-password", secret: true);

        var postgres = distributedApplicationBuilder.AddPostgres("postgres", dbUsername, dbPassword)
            .WithDataVolume(isReadOnly: false);

        postgres.WithPgAdmin(pgAdmin => pgAdmin.WithParentRelationship(postgres.Resource));

        return postgres.AddDatabase("byakko-db");
    }

    internal static IResourceBuilder<ILocalStackResource> BuildLocalStack(this IDistributedApplicationBuilder builder)
    {
        var localstack = builder.AddLocalStack("localstack", configureContainer: container =>
        {
            container.Port = LocalStackPort;
            container.AdditionalEnvironmentVariables.Add("SERVICES", "s3");
            container.Lifetime = ContainerLifetime.Persistent;
            container.DebugLevel = 1;
            container.LogLevel = LocalStackLogLevel.Debug;
        }) ?? throw new InvalidOperationException("Failed to add localstack resource");
        
        builder.UseLocalStack(localstack);

        builder.AddContainer("localstack-explorer", DockerImages.LocalStackExplorer)
            .WithHttpEndpoint(targetPort: 3001)
            .WithEnvironment("LOCALSTACK_ENDPOINT", $"http://host.docker.internal:{LocalStackPort.ToString()}")
            .WithParentRelationship(localstack);

        return localstack;
    }

    internal static IResourceBuilder<KeycloakResource> BuildKeyCloak(this IDistributedApplicationBuilder builder)
    {
        var keyCloakUsername = builder.AddParameter("keycloak-username", secret: true);
        var keyCloakPassword = builder.AddParameter("keycloak-password", secret: true);

        return builder.AddKeycloak("keycloak", KeyCloakPort, keyCloakUsername, keyCloakPassword)
            .WithHttpsEndpoint(KeyCloakSecurePort)
            .WithRealmImport("./Configurations/KeyCloak/MadWorld-realm.json")
            .WithBindMount("./Configurations/KeyCloak/themes", "/opt/keycloak/themes")
            .WithDataVolume();
    }

    internal static IResourceBuilder<MailPitContainerResource> BuildMailPit(this IDistributedApplicationBuilder builder)
    {
        return builder.AddMailPit("mailpit");
    }
}